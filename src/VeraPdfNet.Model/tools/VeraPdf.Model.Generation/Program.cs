using VeraPdf.Model.Generation;

try
{
    if (args.Length is not (4 or 5) || args[0] != "--input" || args[2] != "--output" ||
        (args.Length == 5 && args[4] != "--check"))
    {
        Console.Error.WriteLine("Usage: --input <mdl-directory> --output <Generated-directory> [--check]");
        return 2;
    }
    var paths = Directory.GetFiles(args[1], "*.mdl").Order(StringComparer.Ordinal).ToArray();
    if (paths.Length == 0) throw new FormatException("No .mdl inputs found.");
    var entities = paths.SelectMany(p => DslReader.Read(File.ReadAllText(p), Path.GetFileName(p))).ToArray();
    var differences = GeneratedFiles.Apply(args[3], ModelEmitter.Emit(entities), args.Length == 5);
    if (args.Length == 5 && differences.Count > 0)
    {
        foreach (var difference in differences) Console.Error.WriteLine(difference);
        return 1;
    }
    Console.WriteLine($"{entities.Length} model types; {entities.Sum(e => e.Properties.Count)} properties; {entities.Sum(e => e.Links.Count)} named links. " +
        (args.Length == 5 ? "Generated output is current." : "Generated output updated."));
    return 0;
}
catch (Exception error) when (error is FormatException or IOException or ArgumentException or UnauthorizedAccessException)
{
    Console.Error.WriteLine(error.Message);
    return 2;
}
