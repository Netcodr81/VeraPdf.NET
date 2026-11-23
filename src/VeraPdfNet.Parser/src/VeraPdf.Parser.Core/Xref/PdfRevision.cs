using VeraPdf.Parser.Cos;

namespace VeraPdf.Parser.Xref;

public sealed record PdfRevision(long StartXref, CosDictionary Trailer, PdfXrefTable Table);
