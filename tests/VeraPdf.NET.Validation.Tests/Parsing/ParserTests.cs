using System;
using System.Collections.Generic;
using System.Text;
using Shouldly;
using VeraPdf.NET.Parser;
using VeraPdf.NET.Parser.Results;

namespace VeraPdf.NET.Validation.Tests.Parsing;

public class ParserTests
{
    [Fact]
    public void Parse_Should_Extract_Structural_Fields_For_Minimal_Valid_Pdf()
    {
        var pdfBytes = BuildMinimalPdf();
        var parser = new PdfParser();

        var result = parser.Parse(pdfBytes);

        result.Success.ShouldBeTrue();
        result.Document.ShouldNotBeNull();
        result.Document!.Header.ShouldStartWith("%PDF-");
        result.Document.HeaderOffset.ShouldBe(0);
        result.Document.HeaderVersion.ShouldBe(1.7m);
        result.Document.NrIndirects.ShouldBeGreaterThanOrEqualTo(2);
        result.Diagnostics.Any(d => d.Severity == ParseDiagnosticSeverity.Error).ShouldBeFalse();
    }

    [Fact]
    public void Parse_Should_Map_Additional_Document_Markers_Into_Model()
    {
        var pdfBytes = BuildMarkerRichPdf();
        var parser = new PdfParser();

        var result = parser.Parse(pdfBytes);

        result.Success.ShouldBeTrue();
        result.Document.ShouldNotBeNull();

        var doc = result.Document!;

        doc.ContainsInfo.ShouldBeTrue();
        doc.ContainsPieceInfo.ShouldBeTrue();
        doc.ContainsEmbeddedFiles.ShouldBeTrue();
        doc.Marked.ShouldBeTrue();
        doc.Suspects.ShouldBeTrue();
        doc.DisplayDocTitle.ShouldBeTrue();

        doc.HeaderByte1.ShouldBe((byte)'A');
        doc.HeaderByte2.ShouldBe((byte)'B');
        doc.HeaderByte3.ShouldBe((byte)'C');
        doc.HeaderByte4.ShouldBe((byte)'D');

        doc.Document.ShouldNotBeNull();
        doc.Document!.ContainsMetadata.ShouldBeTrue();
        doc.Document.ContainsXRefStream.ShouldBeTrue();

        doc.Trailer.ShouldNotBeNull();
        doc.Trailer!.IsEncrypted.ShouldBeTrue();
        doc.Trailer.Info.ShouldNotBeNull();
        doc.Trailer.Size.ShouldBe(6);
        doc.Trailer.KeysString.ShouldContain("Size");
        doc.Trailer.KeysString.ShouldContain("Encrypt");
        doc.FirstPageID.ShouldBe("0011223344556677");
        doc.LastID.ShouldBe("8899AABBCCDDEEFF");

        doc.XRef.ShouldNotBeNull();
        doc.XRef!.SubsectionHeaderSpaceSeparated.ShouldBeTrue();
        doc.XRef.XrefEOLMarkersComplyPDFA.ShouldBeTrue();

        doc.MarkInfo.ShouldContain("/Marked true");
        doc.ViewerPreferences.ShouldContain("/DisplayDocTitle true");
    }

    [Fact]
    public void Parse_Should_Fail_When_Header_Is_Missing_In_Strict_Mode()
    {
        var pdfBytes = System.Text.Encoding.ASCII.GetBytes("1 0 obj\n<<>>\nendobj\n%%EOF");
        var parser = new PdfParser();

        var result = parser.Parse(pdfBytes, new PdfParseOptions
        {
            StrictHeaderCheck = true
        });

        result.Success.ShouldBeFalse();
        result.Document.ShouldBeNull();
        result.Diagnostics.Any(d => d.Code == "PDF001" && d.Severity == ParseDiagnosticSeverity.Error).ShouldBeTrue();
    }

    [Fact]
    public void Parse_Should_Report_Warning_For_Missing_StartXref_When_Not_Required()
    {
        var pdfBytes = System.Text.Encoding.ASCII.GetBytes("%PDF-1.7\n1 0 obj\n<< /Type /Catalog >>\nendobj\n%%EOF");
        var parser = new PdfParser();

        var result = parser.Parse(pdfBytes, new PdfParseOptions
        {
            RequireStartXref = false
        });

        result.Success.ShouldBeTrue();
        result.Document.ShouldNotBeNull();
        result.Diagnostics.Any(d => d.Code == "PDF005" && d.Severity == ParseDiagnosticSeverity.Warning).ShouldBeTrue();
    }

    [Fact]
    public async Task ParseAsync_Should_Match_Parse_Output()
    {
        var pdfBytes = BuildMinimalPdf();
        var parser = new PdfParser();

        using var stream = new MemoryStream(pdfBytes);

        var asyncResult = await parser.ParseAsync(stream);
        var syncResult = parser.Parse(pdfBytes);

        asyncResult.Success.ShouldBe(syncResult.Success);
        asyncResult.Document.ShouldNotBeNull();
        syncResult.Document.ShouldNotBeNull();
        asyncResult.Document!.NrIndirects.ShouldBe(syncResult.Document!.NrIndirects);
    }

    [Fact]
    public void ParseFile_Should_Parse_Absolute_Path()
    {
        var parser = new PdfParser();
        var tempFile = Path.Combine(Path.GetTempPath(), $"verapdf-parser-{Guid.NewGuid():N}.pdf");

        try
        {
            File.WriteAllBytes(tempFile, BuildMinimalPdf());

            var result = parser.ParseFile(tempFile);

            result.Success.ShouldBeTrue();
            result.Document.ShouldNotBeNull();
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    private static byte[] BuildMinimalPdf()
    {
        const string pdf = "%PDF-1.7\n" +
                           "1 0 obj\n" +
                           "<< /Type /Catalog /Pages 2 0 R >>\n" +
                           "endobj\n" +
                           "2 0 obj\n" +
                           "<< /Type /Pages /Count 0 /Kids [] >>\n" +
                           "endobj\n" +
                           "xref\n" +
                           "0 3\n" +
                           "0000000000 65535 f \n" +
                           "0000000010 00000 n \n" +
                           "0000000060 00000 n \n" +
                           "trailer\n" +
                           "<< /Size 3 /Root 1 0 R >>\n" +
                           "startxref\n" +
                           "120\n" +
                           "%%EOF\n";

        return System.Text.Encoding.ASCII.GetBytes(pdf);
    }

    private static byte[] BuildMarkerRichPdf()
    {
        const string pdf = "%PDF-1.7\n" +
                           "%ABCD\n" +
                           "1 0 obj\n" +
                           "<< /Type /Catalog /Pages 2 0 R /PieceInfo <<>> /Metadata 4 0 R /MarkInfo << /Marked true /Suspects true >> /ViewerPreferences << /DisplayDocTitle true >> >>\n" +
                           "endobj\n" +
                           "2 0 obj\n" +
                           "<< /Type /Pages /Count 0 /Kids [] >>\n" +
                           "endobj\n" +
                           "3 0 obj\n" +
                           "<< /Type /XRef /Info 5 0 R /EmbeddedFiles <<>> >>\n" +
                           "endobj\n" +
                           "xref\n" +
                           "0 6\n" +
                           "0000000000 65535 f \n" +
                           "0000000010 00000 n \n" +
                           "0000000110 00000 n \n" +
                           "trailer\n" +
                           "<< /Size 6 /Root 1 0 R /Info 5 0 R /Encrypt 7 0 R /ID [<0011223344556677> <8899AABBCCDDEEFF>] >>\n" +
                           "startxref\n" +
                           "42\n" +
                           "%%EOF\n";

        return System.Text.Encoding.ASCII.GetBytes(pdf);
    }
}
