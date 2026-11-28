namespace VeraPdf.Library.Core.Utils;

public sealed class LogsFormatter
{
    public string Format(string? sourceClassName, string? sourceMethodName, string? level, string? message) =>
        $"[log]{Environment.NewLine}" +
        $"{Display(sourceClassName)} {Display(sourceMethodName)} in process {Environment.ProcessId}{Environment.NewLine}" +
        $"{Display(level)}: {Display(message)}{Environment.NewLine}";

    private static string Display(string? value) => value ?? "null";
}
