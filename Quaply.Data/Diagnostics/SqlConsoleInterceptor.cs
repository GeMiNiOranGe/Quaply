#if DEBUG
using System.Data.Common;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Quaply.Data.Diagnostics;

/// <summary>
/// Logs EF commands using the original EF Core style, with:
/// <list type="bullet">
///   <item>
///   more precise timing (fractional milliseconds)
///   </item>
///   <item>
///   parameters listed with their actual values
///   </item>
///   <item>
///   formatted SQL: each SELECT column is placed on a separate line
///   when there is more than one column.
///   </item>
/// </list>
/// </summary>
public sealed partial class SqlConsoleInterceptor(ILoggerFactory loggerFactory)
    : DbCommandInterceptor
{
    // Set to false to keep @p0, @p1 in the SQL instead of inlining their values.
    private const bool InlineParameterValues = true;

    private const string EfCategory =
        "Microsoft.EntityFrameworkCore.Database.Command";

    private const string Indent = "    ";

    private readonly ILogger _logger = loggerFactory.CreateLogger(EfCategory);

    private static readonly EventId ExecutedId = new(20101, "CommandExecuted");

    private static readonly EventId ErrorId = new(20102, "CommandError");

    [GeneratedRegex(
        @"^(SELECT(?:\s+DISTINCT)?)\s+(.+)$",
        RegexOptions.IgnoreCase
    )]
    private static partial Regex SelectLineRegex();

    // Matches: INSERT INTO "Table" ("A", "B", ...)
    [GeneratedRegex(
        @"^INSERT\s+INTO\s+(.+?)\s*\((.+)\)$",
        RegexOptions.IgnoreCase
    )]
    private static partial Regex InsertIntoRegex();

    // Matches: VALUES (v1, v2, ...)  (optional trailing semicolon)
    [GeneratedRegex(@"^VALUES\s*\((.+)\)(;?)$", RegexOptions.IgnoreCase)]
    private static partial Regex ValuesRegex();

    // Matches: RETURNING "A", "B"  (optional trailing semicolon)
    [GeneratedRegex(@"^RETURNING\s+(.+?)(;?)$", RegexOptions.IgnoreCase)]
    private static partial Regex ReturningRegex();

    // Matches: UPDATE "Table" SET <assignments>
    // or (EF may already place SET on its own line): "SET <assignments>"
    [GeneratedRegex(@"^(UPDATE\s+.+?)\s+SET\s+(.+)$", RegexOptions.IgnoreCase)]
    private static partial Regex UpdateSetInlineRegex();

    [GeneratedRegex(@"^SET\s+(.+)$", RegexOptions.IgnoreCase)]
    private static partial Regex SetLineRegex();

    // --- Reader ---

    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result
    )
    {
        LogExecuted(command, eventData);
        return result;
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default
    )
    {
        LogExecuted(command, eventData);
        return new(result);
    }

    // --- NonQuery (INSERT/UPDATE/DELETE) ---

    public override int NonQueryExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result
    )
    {
        LogExecuted(command, eventData, $"{result} row(s) affected");
        return result;
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default
    )
    {
        LogExecuted(command, eventData, $"{result} row(s) affected");
        return new(result);
    }

    // --- Scalar ---

    public override object? ScalarExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result
    )
    {
        LogExecuted(command, eventData);
        return result;
    }

    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default
    )
    {
        LogExecuted(command, eventData);
        return new(result);
    }

    // --- Error ---

    public override void CommandFailed(
        DbCommand command,
        CommandErrorEventData eventData
    )
    {
        LogFailed(command, eventData);
    }

    public override Task CommandFailedAsync(
        DbCommand command,
        CommandErrorEventData eventData,
        CancellationToken cancellationToken = default
    )
    {
        LogFailed(command, eventData);
        return Task.CompletedTask;
    }

    // Utility methods for logging

    private void LogExecuted(
        DbCommand command,
        CommandExecutedEventData e,
        string? note = null
    )
    {
        string message = BuildMessage(
            "Executed DbCommand",
            command,
            e.Duration,
            note
        );
        _logger.Log(LogLevel.Information, ExecutedId, "{Message}", message);
    }

    private void LogFailed(DbCommand command, CommandErrorEventData e)
    {
        string message = BuildMessage(
            "Failed executing DbCommand",
            command,
            e.Duration
        );
        _logger.Log(LogLevel.Error, ErrorId, e.Exception, "{Message}", message);
    }

    private static string BuildMessage(
        string title,
        DbCommand command,
        TimeSpan duration,
        string? note = null
    )
    {
        string parameters = string.Join(
            ", ",
            command.Parameters.Cast<DbParameter>().Select(FormatParameter)
        );

        StringBuilder sb = new();

        sb.Append(
                $"{title} ({duration.TotalMilliseconds.ToString("0.##", CultureInfo.InvariantCulture)}ms) "
            )
            .Append($"[Parameters=[{parameters}], ")
            .Append($"CommandType='{command.CommandType}', ")
            .Append($"CommandTimeout='{command.CommandTimeout}']")
            .Append('\n');

        string sql = InlineParameterValues
            ? InlineParameters(command)
            : command.CommandText;
        sb.Append(FormatInsert(FormatUpdate(FormatSelect(sql))));

        if (note is not null)
        {
            sb.Append("\n-- ").Append(note);
        }

        // The console logger only indents lines separated by Environment.NewLine.
        return sb.ToString().ReplaceLineEndings();
    }

    // --- Format methods ---

    private static string FormatParameter(DbParameter p)
    {
        string value = p.Value switch
        {
            null or DBNull => "NULL",
            bool b => $"'{(b ? "True" : "False")}'",
            IFormattable f =>
                $"'{f.ToString(null, CultureInfo.InvariantCulture)}'",
            _ => $"'{p.Value}'",
        };

        return $"{NormalizeName(p.ParameterName)}={value} (DbType = {p.DbType})";
    }

    /// <summary>
    /// EF always places the SELECT column list on a single line (with FROM
    /// on the next line), including inside subqueries (with indentation).
    /// Process each line individually.
    /// </summary>
    private static string FormatSelect(string sql)
    {
        StringBuilder sb = new();
        string[] lines = sql.ReplaceLineEndings("\n").Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            string trimmed = line.TrimStart();
            string indent = line[..(line.Length - trimmed.Length)];

            Match match = SelectLineRegex().Match(trimmed);
            List<string> columns = match.Success
                ? SplitTopLevel(match.Groups[2].Value)
                : [];

            if (columns.Count > 1)
            {
                sb.Append(indent).Append(match.Groups[1].Value);
                foreach (string column in columns)
                {
                    sb.Append('\n')
                        .Append(indent)
                        .Append(Indent)
                        .Append(column);
                    if (!ReferenceEquals(column, columns[^1]))
                    {
                        sb.Append(',');
                    }
                }
            }
            else
            {
                sb.Append(line);
            }

            if (i < lines.Length - 1)
            {
                sb.Append('\n');
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats INSERT statements:
    /// <list type="bullet">
    ///   <item>
    ///   More than one column: INSERT columns, VALUES and RETURNING are each
    ///   written one item per line, wrapped in parentheses (except RETURNING).
    ///   </item>
    ///   <item>
    ///   A single column: the statement stays in EF's original compact form.
    ///   </item>
    /// </list>
    /// </summary>
    private static string FormatInsert(string sql)
    {
        StringBuilder sb = new();
        string[] lines = sql.ReplaceLineEndings("\n").Split('\n');

        // VALUES follows INSERT, so it uses the same decision as the INSERT line.
        bool multiColumnInsert = false;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            string trimmed = line.TrimStart();
            string indent = line[..(line.Length - trimmed.Length)];
            string itemIndent = indent + Indent;

            Match insert = InsertIntoRegex().Match(trimmed);
            Match values = ValuesRegex().Match(trimmed);
            Match returning = ReturningRegex().Match(trimmed);

            if (insert.Success)
            {
                List<string> columns = SplitTopLevel(insert.Groups[2].Value);
                multiColumnInsert = columns.Count > 1;

                if (multiColumnInsert)
                {
                    sb.Append(indent)
                        .Append("INSERT INTO ")
                        .Append(insert.Groups[1].Value)
                        .Append('\n')
                        .Append(indent)
                        .Append("(\n")
                        .Append(
                            string.Join(
                                ",\n",
                                columns.Select(c => itemIndent + c)
                            )
                        )
                        .Append('\n')
                        .Append(indent)
                        .Append(')');
                }
                else
                {
                    sb.Append(line);
                }
            }
            else if (values.Success && multiColumnInsert)
            {
                List<string> items = SplitTopLevel(values.Groups[1].Value);

                sb.Append(indent)
                    .Append("VALUES\n")
                    .Append(indent)
                    .Append("(\n")
                    .Append(
                        string.Join(",\n", items.Select(v => itemIndent + v))
                    )
                    .Append('\n')
                    .Append(indent)
                    .Append(')')
                    .Append(values.Groups[2].Value);
            }
            else if (returning.Success)
            {
                List<string> columns = SplitTopLevel(returning.Groups[1].Value);

                if (columns.Count > 1)
                {
                    sb.Append(indent)
                        .Append("RETURNING\n")
                        .Append(
                            string.Join(
                                ",\n",
                                columns.Select(c => itemIndent + c)
                            )
                        )
                        .Append(returning.Groups[2].Value);
                }
                else
                {
                    sb.Append(line);
                }
            }
            else
            {
                sb.Append(line);
            }

            if (i < lines.Length - 1)
            {
                sb.Append('\n');
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats UPDATE statements:
    /// <list type="bullet">
    ///   <item>SET is always on a new line.</item>
    ///   <item>
    ///   One assignment per line when there is more than one column;
    ///   a single assignment stays on the SET line.
    ///   </item>
    /// </list>
    /// </summary>
    private static string FormatUpdate(string sql)
    {
        StringBuilder sb = new();
        string[] lines = sql.ReplaceLineEndings("\n").Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            string trimmed = line.TrimStart();
            string indent = line[..(line.Length - trimmed.Length)];

            string? header = null; // "UPDATE ..." part when SET is on the same line
            string? assignments = null;

            Match inline = UpdateSetInlineRegex().Match(trimmed);
            if (inline.Success)
            {
                header = inline.Groups[1].Value;
                assignments = inline.Groups[2].Value;
            }
            else
            {
                Match setOnly = SetLineRegex().Match(trimmed);
                if (setOnly.Success)
                {
                    assignments = setOnly.Groups[1].Value;
                }
            }

            if (assignments is null)
            {
                sb.Append(line);
            }
            else
            {
                if (header is not null)
                {
                    sb.Append(indent).Append(header).Append('\n');
                }

                List<string> columns = SplitTopLevel(assignments);

                if (columns.Count > 1)
                {
                    // First assignment shares the line with SET,
                    // the rest are aligned underneath it.
                    string pad = new(' ', "SET ".Length);

                    for (int c = 0; c < columns.Count; c++)
                    {
                        sb.Append(indent);
                        sb.Append(c == 0 ? "SET " : pad);
                        sb.Append(columns[c]);
                        if (c < columns.Count - 1)
                        {
                            sb.Append(",\n");
                        }
                    }
                }
                else
                {
                    sb.Append(indent).Append("SET ").Append(assignments.Trim());
                }
            }

            if (i < lines.Length - 1)
            {
                sb.Append('\n');
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Splits on commas at the top level (ignoring parentheses and strings/identifiers).
    /// </summary>
    private static List<string> SplitTopLevel(string text)
    {
        List<string> parts = [];
        StringBuilder current = new();
        int depth = 0;
        char quote = '\0';

        foreach (char c in text)
        {
            if (quote != '\0')
            {
                current.Append(c);
                if (c == quote)
                {
                    // Escaped '' or "" will close and reopen naturally,
                    // which is still handled correctly.
                    quote = '\0';
                }
                continue;
            }

            switch (c)
            {
                case '\'' or '"':
                    quote = c;
                    current.Append(c);
                    break;
                case '(':
                    depth++;
                    current.Append(c);
                    break;
                case ')':
                    depth--;
                    current.Append(c);
                    break;
                case ',' when depth == 0:
                    parts.Add(current.ToString().Trim());
                    current.Clear();
                    break;
                default:
                    current.Append(c);
                    break;
            }
        }

        if (current.Length > 0)
        {
            parts.Add(current.ToString().Trim());
        }

        return parts;
    }

    // --- Parameters ---

    private static string NormalizeName(string name)
    {
        return name.Length > 0 && name[0] is not ('@' or ':' or '$')
            ? "@" + name
            : name;
    }

    private static string InlineParameters(DbCommand command)
    {
        string sql = command.CommandText;

        // Prevent @p1 from matching @p10
        foreach (
            DbParameter p in command
                .Parameters.Cast<DbParameter>()
                .OrderByDescending(p => p.ParameterName.Length)
        )
        {
            string literal = ToLiteral(p.Value);
            sql = Regex.Replace(
                sql,
                Regex.Escape(NormalizeName(p.ParameterName)) + @"\b",
                _ => literal
            );
        }

        return sql;
    }

    private static string ToLiteral(object? value)
    {
        return value switch
        {
            null or DBNull => "NULL",
            string s => $"'{s.Replace("'", "''")}'",
            bool b => b ? "1" : "0",
            byte[] bytes => $"X'{Convert.ToHexString(bytes)}'",
            DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss.FFFFFFF}'",
            DateTimeOffset dto => $"'{dto:yyyy-MM-dd HH:mm:ss.FFFFFFFzzz}'",
            Guid g => $"'{g.ToString().ToUpperInvariant()}'",
            DateOnly d => $"'{d:yyyy-MM-dd}'",
            TimeOnly t => $"'{t:HH:mm:ss.FFFFFFF}'",
            TimeSpan ts => $"'{ts:c}'",
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => $"'{value.ToString()?.Replace("'", "''")}'",
        };
    }
}
#endif
