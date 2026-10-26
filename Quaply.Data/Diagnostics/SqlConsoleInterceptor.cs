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
        sb.Append(FormatSelect(sql));

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
