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

    // bright blue
    private const string KeywordColor = "\e[94m";

    // bright green
    private const string StringColor = "\e[92m";

    // bright yellow
    private const string NumberColor = "\e[93m";

    private const string Reset = "\e[0m";

    private readonly ILogger _logger = loggerFactory.CreateLogger(EfCategory);

    private static readonly bool UseColor =
        !Console.IsOutputRedirected
        && Environment.GetEnvironmentVariable("NO_COLOR") is null;

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

    // The alternation order is important: strings/identifiers must be matched first
    // so that keywords contained within them are not incorrectly highlighted.
    [GeneratedRegex(
        @"(?<str>'(?:[^']|'')*')" // string literal -> skip
            + @"|""[^""]*""" // "Identifier"   -> skip
            + @"|\b(?<kw>SELECT|DISTINCT|FROM|WHERE|AND|OR|NOT|IN|IS|NULL|LIKE"
            + @"|INNER|LEFT|RIGHT|CROSS|OUTER|JOIN|ON|AS|ORDER|GROUP|BY|HAVING"
            + @"|LIMIT|OFFSET|INSERT|INTO|VALUES|UPDATE|SET|DELETE|RETURNING"
            + @"|CASE|WHEN|THEN|ELSE|END|EXISTS|UNION|ALL|ASC|DESC"
            + @"|BEGIN|COMMIT|ROLLBACK|PRAGMA)\b"
            + @"|(?<num>\b\d+(?:\.\d+)?\b)",
        RegexOptions.IgnoreCase
    )]
    private static partial Regex SqlTokenRegex();

    [GeneratedRegex(@"[@:$]\w+")]
    private static partial Regex ParameterRegex();

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
        if (!_logger.IsEnabled(LogLevel.Information))
        {
            return;
        }

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
        if (!_logger.IsEnabled(LogLevel.Error))
        {
            return;
        }

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
        sb.Append(Highlight(FormatSql(sql)));

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
    /// Formats SELECT, INSERT and UPDATE statements in a single pass over the
    /// lines. Each line is offered to the statement-specific handlers in turn;
    /// the first handler that recognises it writes the formatted output.
    /// </summary>
    private static string FormatSql(string sql)
    {
        string[] lines = sql.ReplaceLineEndings("\n").Split('\n');
        StringBuilder sb = new(sql.Length + 128);

        // VALUES follows INSERT, so it reuses the decision made on the INSERT line.
        bool multiColumnInsert = false;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            string trimmed = line.TrimStart();
            string indent = line[..(line.Length - trimmed.Length)];

            bool handled =
                TryFormatSelect(sb, trimmed, indent)
                || TryFormatInsertInto(
                    sb,
                    line,
                    trimmed,
                    indent,
                    ref multiColumnInsert
                )
                || (multiColumnInsert && TryFormatValues(sb, trimmed, indent))
                || TryFormatReturning(sb, trimmed, indent)
                || TryFormatUpdate(sb, trimmed, indent);

            if (!handled)
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

    // --- Line handlers: return true when they wrote the line to `sb` ---

    /// <summary>
    /// EF always places the SELECT column list on a single line (with FROM
    /// on the next line), including inside subqueries (with indentation).
    /// A column list with more than one column is split one per line.
    /// </summary>
    private static bool TryFormatSelect(
        StringBuilder sb,
        string trimmed,
        string indent
    )
    {
        Match match = SelectLineRegex().Match(trimmed);
        if (!match.Success)
        {
            return false;
        }

        List<string> columns = SplitTopLevel(match.Groups[2].Value);
        if (columns.Count <= 1)
        {
            return false;
        }

        sb.Append(indent).Append(match.Groups[1].Value);
        for (int c = 0; c < columns.Count; c++)
        {
            sb.Append('\n').Append(indent).Append(Indent).Append(columns[c]);
            if (c < columns.Count - 1)
            {
                sb.Append(',');
            }
        }

        return true;
    }

    /// <summary>
    /// INSERT INTO "Table" ("A", "B") becomes one column per line wrapped in
    /// parentheses. A single column keeps EF's compact form.
    /// </summary>
    private static bool TryFormatInsertInto(
        StringBuilder sb,
        string line,
        string trimmed,
        string indent,
        ref bool multiColumnInsert
    )
    {
        Match match = InsertIntoRegex().Match(trimmed);
        if (!match.Success)
        {
            return false;
        }

        List<string> columns = SplitTopLevel(match.Groups[2].Value);
        multiColumnInsert = columns.Count > 1;

        if (!multiColumnInsert)
        {
            sb.Append(line);
            return true;
        }

        sb.Append(indent)
            .Append("INSERT INTO ")
            .Append(match.Groups[1].Value)
            .Append('\n')
            .Append(indent)
            .Append("(\n");
        AppendItems(sb, columns, indent + Indent);
        sb.Append('\n').Append(indent).Append(')');

        return true;
    }

    private static bool TryFormatValues(
        StringBuilder sb,
        string trimmed,
        string indent
    )
    {
        Match match = ValuesRegex().Match(trimmed);
        if (!match.Success)
        {
            return false;
        }

        List<string> items = SplitTopLevel(match.Groups[1].Value);

        sb.Append(indent).Append("VALUES\n").Append(indent).Append("(\n");
        AppendItems(sb, items, indent + Indent);
        sb.Append('\n')
            .Append(indent)
            .Append(')')
            .Append(match.Groups[2].Value);

        return true;
    }

    private static bool TryFormatReturning(
        StringBuilder sb,
        string trimmed,
        string indent
    )
    {
        Match match = ReturningRegex().Match(trimmed);
        if (!match.Success)
        {
            return false;
        }

        List<string> columns = SplitTopLevel(match.Groups[1].Value);
        if (columns.Count <= 1)
        {
            return false;
        }

        sb.Append(indent).Append("RETURNING\n");
        AppendItems(sb, columns, indent + Indent);
        sb.Append(match.Groups[2].Value);

        return true;
    }

    /// <summary>
    /// SET is always on a new line. One assignment per line when there is more
    /// than one; a single assignment stays on the SET line.
    /// </summary>
    private static bool TryFormatUpdate(
        StringBuilder sb,
        string trimmed,
        string indent
    )
    {
        // "UPDATE ..." part when SET is on the same line
        string? header = null;
        string assignments;

        Match inline = UpdateSetInlineRegex().Match(trimmed);
        if (inline.Success)
        {
            header = inline.Groups[1].Value;
            assignments = inline.Groups[2].Value;
        }
        else
        {
            Match setOnly = SetLineRegex().Match(trimmed);
            if (!setOnly.Success)
            {
                return false;
            }

            assignments = setOnly.Groups[1].Value;
        }

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
                sb.Append(indent)
                    .Append(c == 0 ? "SET " : pad)
                    .Append(columns[c]);
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

        return true;
    }

    /// <summary>
    /// Appends items one per line, separated by ",\n".
    /// </summary>
    private static void AppendItems(
        StringBuilder sb,
        List<string> items,
        string itemIndent
    )
    {
        for (int c = 0; c < items.Count; c++)
        {
            if (c > 0)
            {
                sb.Append(",\n");
            }

            sb.Append(itemIndent).Append(items[c]);
        }
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

    // --- Highlight ---

    private static string Highlight(string sql)
    {
        if (!UseColor)
        {
            return sql;
        }

        return SqlTokenRegex()
            .Replace(
                sql,
                m =>
                    m.Groups["kw"].Success ? $"{KeywordColor}{m.Value}{Reset}"
                    : m.Groups["str"].Success ? $"{StringColor}{m.Value}{Reset}"
                    : m.Groups["num"].Success ? $"{NumberColor}{m.Value}{Reset}"
                    : m.Value
            );
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
        Dictionary<string, string> literals = command
            .Parameters.Cast<DbParameter>()
            .ToDictionary(
                p => NormalizeName(p.ParameterName),
                p => ToLiteral(p.Value)
            );

        return ParameterRegex()
            .Replace(
                command.CommandText,
                m =>
                    literals.TryGetValue(m.Value, out string? literal)
                        ? literal
                        : m.Value
            );
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
