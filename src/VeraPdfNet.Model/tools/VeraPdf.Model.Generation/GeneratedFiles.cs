using System.Text;

namespace VeraPdf.Model.Generation;

public static class GeneratedFiles
{
    public static IReadOnlyList<string> Apply(string directory, IReadOnlyDictionary<string, string> expected, bool check)
    {
        var root = Path.GetFullPath(directory);
        if (!string.Equals(Path.GetFileName(root), "Generated", StringComparison.Ordinal))
        {
            throw new ArgumentException("Output must be a dedicated directory named Generated.", nameof(directory));
        }
        for (var ancestor = new DirectoryInfo(root); ancestor is not null; ancestor = ancestor.Parent)
        {
            if (ancestor.Exists && ancestor.Attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                throw new IOException($"Generation cannot write through a linked directory: {ancestor.FullName}");
            }
        }
        foreach (var name in expected.Keys)
        {
            if (Path.GetFileName(name) != name || name is "." or "..")
            {
                throw new ArgumentException($"Generated filename must not escape its output directory: {name}");
            }
        }
        var existing = Directory.Exists(root) ? Directory.GetFiles(root) : [];
        foreach (var file in existing)
        {
            if (File.GetAttributes(file).HasFlag(FileAttributes.ReparsePoint))
            {
                throw new IOException($"Generation cannot overwrite linked files: {file}");
            }
        }
        var owned = existing.Where(p => p.EndsWith(".g.cs", StringComparison.Ordinal) ||
            Path.GetFileName(p) == "schema.json").ToArray();
        var differences = new List<string>();
        foreach (var pair in expected)
        {
            var path = Path.Combine(root, pair.Key);
            var current = File.Exists(path) ? File.ReadAllText(path) : null;
            if (current == pair.Value) continue;
            differences.Add((current is null ? "Missing: " : "Changed: ") + pair.Key);
        }
        foreach (var file in owned)
        {
            if (!expected.ContainsKey(Path.GetFileName(file)))
            {
                differences.Add("Obsolete: " + Path.GetFileName(file));
            }
            if (file.EndsWith(".g.cs", StringComparison.Ordinal) &&
                !File.ReadAllText(file).StartsWith(ModelEmitter.Banner, StringComparison.Ordinal))
            {
                throw new IOException($"Refusing to touch a file not owned by this generator: {file}");
            }
        }
        if (check) return differences;
        Directory.CreateDirectory(root);
        foreach (var pair in expected)
        {
            var path = Path.Combine(root, pair.Key);
            if (!File.Exists(path) || File.ReadAllText(path) != pair.Value)
            {
                File.WriteAllText(path, pair.Value, new UTF8Encoding(false));
            }
        }
        foreach (var file in owned.Where(p => !expected.ContainsKey(Path.GetFileName(p)))) File.Delete(file);
        return differences;
    }
}
