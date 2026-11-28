using System.Collections.Frozen;
using System.Diagnostics;
using System.Text;
using VeraPdf.Library.Core.Utils;

namespace VeraPdf.Library.Components;

public sealed class Log
{
    private Log(int occurrences, string level, string message)
    {
        Occurrences = occurrences;
        Level = level;
        Message = message;
    }

    public int Occurrences { get; }
    public string Level { get; }
    public string Message { get; }

    public static Log FromValues(int occurrences, string level, string message)
    {
        if (occurrences <= 0)
            throw new ArgumentOutOfRangeException(nameof(occurrences), "Occurrences must be greater than zero.");
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(message);
        return new Log(occurrences, level, message);
    }
}

public sealed class LogsSummary
{
    private LogsSummary(int logsCount, IEnumerable<Log> logs)
    {
        LogsCount = logsCount;
        Logs = logs.ToFrozenSet();
    }

    public int LogsCount { get; }
    public IReadOnlySet<Log> Logs { get; }

    public static LogsSummary Create(int logsCount, IEnumerable<Log> logs)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(logsCount);
        ArgumentNullException.ThrowIfNull(logs);
        var snapshot = logs.ToArray();
        if (snapshot.Any(log => log is null))
            throw new ArgumentException("Log entries cannot contain null.", nameof(logs));
        return new LogsSummary(logsCount, snapshot);
    }

    public static LogsSummary GetSummary()
    {
        try
        {
            return GetSummary(LogsFileHandler.GetLogFile());
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            Trace.TraceWarning(
                $"Failed to get logger messages from file. Warnings report will be empty: {exception.Message}");
            return Create(0, []);
        }
    }

    public static LogsSummary GetSummary(string logFilePath)
    {
        ArgumentNullException.ThrowIfNull(logFilePath);
        var lines = File.ReadAllLines(logFilePath);
        var messages = new List<string>();
        for (var index = 0; index < lines.Length; index++)
        {
            if (!lines[index].Contains("[log]", StringComparison.Ordinal))
            {
                continue;
            }

            if (index + 2 >= lines.Length)
            {
                throw new FormatException("Log entry is missing its source or message line.");
            }

            index += 2;
            var message = new StringBuilder(lines[index]);
            while (index + 1 < lines.Length
                   && !lines[index + 1].Contains("[log]", StringComparison.Ordinal))
            {
                message.Append('\n').Append(lines[++index]);
            }

            messages.Add(message.ToString());
        }

        var occurrences = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var message in messages)
        {
            occurrences.TryGetValue(message, out var count);
            occurrences[message] = count + 1;
        }

        var logs = new List<Log>(occurrences.Count);
        foreach (var (message, count) in occurrences)
        {
            var separatorIndex = message.IndexOf(": ", StringComparison.Ordinal);
            if (separatorIndex < 0)
            {
                throw new FormatException("Log message is missing the level separator ': '.");
            }

            logs.Add(Log.FromValues(
                count,
                message[..separatorIndex],
                message[(separatorIndex + 2)..]));
        }

        return Create(messages.Count, logs);
    }
}
