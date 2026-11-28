using System.Diagnostics;

namespace VeraPdf.Library.Core.Utils;

public static class LogsFileHandler
{
    public const string Suffix = ".log";

    private static readonly object SyncRoot = new();
    private static readonly Dictionary<int, string> LogFilePaths = new();

    static LogsFileHandler()
    {
        AppDomain.CurrentDomain.ProcessExit += DeleteLogFiles;
    }

    public static long GetPid() => Environment.ProcessId;

    public static string GetLogFile()
    {
        var processId = Environment.ProcessId;
        lock (SyncRoot)
        {
            if (LogFilePaths.TryGetValue(processId, out var existingPath))
            {
                return existingPath;
            }

            var path = CreateTemporaryLogFile(processId);
            LogFilePaths.Add(processId, path);
            return path;
        }
    }

    public static void CreateNewLogFile()
    {
        using var stream = new FileStream(GetLogFile(), FileMode.Create, FileAccess.Write, FileShare.Read);
    }

    private static string CreateTemporaryLogFile(int processId)
    {
        while (true)
        {
            var path = Path.Combine(Path.GetTempPath(), $"logs{processId}-{Guid.NewGuid():N}{Suffix}");
            try
            {
                using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
                return path;
            }
            catch (IOException) when (File.Exists(path))
            {
            }
        }
    }

    private static void DeleteLogFiles(object? sender, EventArgs args)
    {
        string[] paths;
        lock (SyncRoot)
        {
            paths = LogFilePaths.Values.ToArray();
        }

        foreach (var path in paths)
        {
            try
            {
                File.Delete(path);
            }
            catch (IOException exception)
            {
                Trace.TraceError($"Unable to delete temporary log file '{path}': {exception.Message}");
            }
            catch (UnauthorizedAccessException exception)
            {
                Trace.TraceError($"Unable to delete temporary log file '{path}': {exception.Message}");
            }
        }
    }
}
