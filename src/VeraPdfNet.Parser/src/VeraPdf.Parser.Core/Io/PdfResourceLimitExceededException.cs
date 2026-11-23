namespace VeraPdf.Parser.Io;

public sealed class PdfResourceLimitExceededException : IOException
{
    public PdfResourceLimitExceededException(string message)
        : base(message)
    {
    }
}
