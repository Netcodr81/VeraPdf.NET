using Shouldly;
using VeraPdf.NET.Validation.Tests.TestModels;
using VeraPdf.NET.Validation.Tests.Utils;

namespace VeraPdf.NET.Validation.Tests.Execution;

public class PdfFileValidationTests
{
    private const string PdfIntegrityRule =
        "HasPdfHeader == true && HasEofMarker == true && HasCatalogObject == true && PageMarkerCount >= 1 && FileSize > 0";

    [Fact]
    public void Should_Mark_Real_Valid_Pdf_As_Compliant()
    {
        var path = TestHelper.GetAssetPath("sample-valid.pdf");
        var snapshot = TestHelper.LoadPdfSnapshot(path);
        var validator = TestHelper.CreateValidator(PdfIntegrityRule, typeof(PdfFileSnapshot));

        var result = validator.Validate(snapshot);

        result.Results.Count.ShouldBe(1);
        result.IsCompliant.ShouldBeTrue();
    }

    [Fact]
    public void Should_Mark_Real_Invalid_Pdf_As_NonCompliant()
    {
        var path = TestHelper.GetAssetPath("sample-invalid.pdf");
        var snapshot = TestHelper.LoadPdfSnapshot(path);
        var validator = TestHelper.CreateValidator(PdfIntegrityRule, typeof(PdfFileSnapshot));

        var result = validator.Validate(snapshot);

        result.Results.Count.ShouldBe(1);
        result.IsCompliant.ShouldBeFalse();
    }
}
