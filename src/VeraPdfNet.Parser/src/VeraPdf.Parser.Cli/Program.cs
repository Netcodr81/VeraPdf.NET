using VeraPdf.Parser;
using VeraPdf.Parser.Pd;

if (args.Length == 0)
{
    Console.WriteLine("VeraPdf.Parser CLI");
    Console.WriteLine("Usage: VeraPdf.Parser.Cli <pdf-path>");
    return;
}

try
{
    using var document = PdfDocument.Open(args[0]);
    Console.WriteLine($"{ParserLibrary.Name}: {document.PageCount} page(s)");
}
catch (Exception exception)
{
    Console.Error.WriteLine($"Cannot open PDF: {exception.Message}");
    Environment.ExitCode = 1;
}
