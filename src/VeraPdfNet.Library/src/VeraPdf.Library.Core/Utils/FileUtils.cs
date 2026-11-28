namespace VeraPdf.Library.Core.Utils;

public static class FileUtils
{
    public static string ExtFromFileName(string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        var lastDot = fileName.LastIndexOf('.');
        if (lastDot < 0)
        {
            return string.Empty;
        }

        return lastDot == fileName.Length - 1 ? fileName : fileName[(lastDot + 1)..];
    }

    public static bool HasExtNoCase(string fileName, string extension)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(extension);
        return HasExt(fileName, extension, StringComparison.OrdinalIgnoreCase);
    }

    public static bool HasExt(string fileName, string extension)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(extension);
        return HasExt(fileName, extension, StringComparison.Ordinal);
    }

    public static string AddExt(string path, string extension)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(extension);
        var lastDot = path.LastIndexOf('.');
        var lastSeparator = Math.Max(path.LastIndexOf('\\'), path.LastIndexOf('/'));
        return lastDot > lastSeparator
            ? path[..(lastDot + 1)] + extension
            : path + "." + extension;
    }

    private static bool HasExt(string fileName, string extension, StringComparison comparison)
    {
        var fullExtension = extension.StartsWith(".", StringComparison.Ordinal) ? extension : "." + extension;
        return fileName.EndsWith(fullExtension, comparison);
    }
}
