using Shouldly;
using VeraPdf.NET.Model;
using VeraPdf.NET.Model.Contracts.External;
using VeraPdf.NET.Model.Contracts.Semantic;
using VeraPdf.NET.Model.Contracts.StructureAnalysis;
using VeraPdf.NET.Model.Contracts.Xmp;
using VeraPdf.NET.Model.Pd.Contracts;
using VeraPdf.NET.Validation.Infrastructure;

namespace VeraPdf.NET.Validation.Tests.Parity;

public class ModelContractParityTests
{
    [Fact]
    public void Model_Should_Implement_Core_ColorSpace_Contracts()
    {
        typeof(IPDColorSpace).IsAssignableFrom(typeof(PDColorSpace)).ShouldBeTrue();
        typeof(IPDDeviceGray).IsAssignableFrom(typeof(PDDeviceGray)).ShouldBeTrue();
        typeof(IPDDeviceRGB).IsAssignableFrom(typeof(PDDeviceRGB)).ShouldBeTrue();
        typeof(IPDDeviceN).IsAssignableFrom(typeof(PDDeviceN)).ShouldBeTrue();
        typeof(IPDICCBased).IsAssignableFrom(typeof(PDICCBased)).ShouldBeTrue();
        typeof(IPDICCBasedCMYK).IsAssignableFrom(typeof(PDICCBasedCMYK)).ShouldBeTrue();
        typeof(IPDIndexed).IsAssignableFrom(typeof(PDIndexed)).ShouldBeTrue();
        typeof(IPDSeparation).IsAssignableFrom(typeof(PDSeparation)).ShouldBeTrue();
    }

    [Fact]
    public void Model_Should_Implement_Core_Font_Contracts()
    {
        typeof(IPDFont).IsAssignableFrom(typeof(PDFont)).ShouldBeTrue();
        typeof(IPDSimpleFont).IsAssignableFrom(typeof(PDSimpleFont)).ShouldBeTrue();
        typeof(IPDTrueTypeFont).IsAssignableFrom(typeof(PDTrueTypeFont)).ShouldBeTrue();
        typeof(IPDType1Font).IsAssignableFrom(typeof(PDType1Font)).ShouldBeTrue();
        typeof(IPDType3Font).IsAssignableFrom(typeof(PDType3Font)).ShouldBeTrue();
        typeof(IPDCIDFont).IsAssignableFrom(typeof(PDCIDFont)).ShouldBeTrue();
        typeof(IPDType0Font).IsAssignableFrom(typeof(PDType0Font)).ShouldBeTrue();
    }

    [Fact]
    public void Model_Should_Implement_External_Contracts()
    {
        typeof(IICCInputProfile).IsAssignableFrom(typeof(ICCInputProfile)).ShouldBeTrue();
        typeof(IFontProgram).IsAssignableFrom(typeof(FontProgram)).ShouldBeTrue();
    }

    [Fact]
    public void Model_Should_Implement_Structure_And_Annotation_Form_Action_Metadata_Contracts()
    {
        typeof(IPDAnnot).IsAssignableFrom(typeof(PDAnnot)).ShouldBeTrue();
        typeof(IPDAcroForm).IsAssignableFrom(typeof(PDAcroForm)).ShouldBeTrue();
        typeof(IPDFormField).IsAssignableFrom(typeof(PDFormField)).ShouldBeTrue();
        typeof(IPDAction).IsAssignableFrom(typeof(PDAction)).ShouldBeTrue();
        typeof(IPDMetadata).IsAssignableFrom(typeof(PDMetadata)).ShouldBeTrue();
        typeof(IPDOutputIntent).IsAssignableFrom(typeof(PDOutputIntent)).ShouldBeTrue();
        typeof(IPDStructTreeRoot).IsAssignableFrom(typeof(PDStructTreeRoot)).ShouldBeTrue();
        typeof(IPDStructElem).IsAssignableFrom(typeof(PDStructElem)).ShouldBeTrue();
    }

    [Fact]
    public void Model_Should_Implement_Xmp_Contracts()
    {
        typeof(IXMPPackage).IsAssignableFrom(typeof(XMPPackage)).ShouldBeTrue();
        typeof(IMainXMPPackage).IsAssignableFrom(typeof(MainXMPPackage)).ShouldBeTrue();
        typeof(IXMPProperty).IsAssignableFrom(typeof(XMPProperty)).ShouldBeTrue();
        typeof(IPDFAIdentification).IsAssignableFrom(typeof(PDFAIdentification)).ShouldBeTrue();
        typeof(IPDFUAIdentification).IsAssignableFrom(typeof(PDFUAIdentification)).ShouldBeTrue();
        typeof(IExtensionSchemasContainer).IsAssignableFrom(typeof(ExtensionSchemasContainer)).ShouldBeTrue();
        typeof(IExtensionSchemaDefinition).IsAssignableFrom(typeof(ExtensionSchemaDefinition)).ShouldBeTrue();
        typeof(IExtensionSchemaProperty).IsAssignableFrom(typeof(ExtensionSchemaProperty)).ShouldBeTrue();
    }

    [Fact]
    public void Model_Should_Implement_Semantic_Content_Contracts()
    {
        typeof(ISEL).IsAssignableFrom(typeof(SEL)).ShouldBeTrue();
        typeof(ISETable).IsAssignableFrom(typeof(SETable)).ShouldBeTrue();
        typeof(ISETableCell).IsAssignableFrom(typeof(SETableCell)).ShouldBeTrue();
        typeof(ISETOCI).IsAssignableFrom(typeof(SETOCI)).ShouldBeTrue();
        typeof(ISEHn).IsAssignableFrom(typeof(SEHn)).ShouldBeTrue();
        typeof(ISEContentItem).IsAssignableFrom(typeof(SEContentItem)).ShouldBeTrue();
        typeof(ISEGroupedContent).IsAssignableFrom(typeof(SEGroupedContent)).ShouldBeTrue();
        typeof(ISESimpleContentItem).IsAssignableFrom(typeof(SESimpleContentItem)).ShouldBeTrue();
    }

    [Fact]
    public void Model_Should_Implement_StructureAnalysis_Contracts()
    {
        typeof(ISAObject).IsAssignableFrom(typeof(SAObject)).ShouldBeTrue();
        typeof(ISAPDFDocument).IsAssignableFrom(typeof(SAPDFDocument)).ShouldBeTrue();
        typeof(ISAStructTreeRoot).IsAssignableFrom(typeof(SAStructTreeRoot)).ShouldBeTrue();
        typeof(ISAStructElem).IsAssignableFrom(typeof(SAStructElem)).ShouldBeTrue();
        typeof(ISAPage).IsAssignableFrom(typeof(SAPage)).ShouldBeTrue();
        typeof(ISAAnnotation).IsAssignableFrom(typeof(SAAnnotation)).ShouldBeTrue();
        typeof(ISALinkAnnotation).IsAssignableFrom(typeof(SALinkAnnotation)).ShouldBeTrue();
        typeof(ISATextChunk).IsAssignableFrom(typeof(SATextChunk)).ShouldBeTrue();
        typeof(ISATableBorder).IsAssignableFrom(typeof(SATableBorder)).ShouldBeTrue();
        typeof(ISATableBorderRow).IsAssignableFrom(typeof(SATableBorderRow)).ShouldBeTrue();
        typeof(ISATableBorderCell).IsAssignableFrom(typeof(SATableBorderCell)).ShouldBeTrue();
        typeof(ISAList).IsAssignableFrom(typeof(SAList)).ShouldBeTrue();
        typeof(ISAListItem).IsAssignableFrom(typeof(SAListItem)).ShouldBeTrue();
    }

    [Fact]
    public void TypeMapper_Should_Resolve_Interface_And_Concrete_Model_Names()
    {
        TypeMapper.MapType("PDDeviceRGB").ShouldBe(typeof(PDDeviceRGB));
        TypeMapper.MapType("IPDDeviceRGB").ShouldBe(typeof(IPDDeviceRGB));
        TypeMapper.MapType("PDFont").ShouldBe(typeof(PDFont));
        TypeMapper.MapType("IPDFont").ShouldBe(typeof(IPDFont));
    }

    [Fact]
    public void TypeMapper_Should_Resolve_New_Interface_Targets()
    {
        TypeMapper.MapType("IPDAnnot").ShouldBe(typeof(IPDAnnot));
        TypeMapper.MapType("IPDAcroForm").ShouldBe(typeof(IPDAcroForm));
        TypeMapper.MapType("IPDFormField").ShouldBe(typeof(IPDFormField));
        TypeMapper.MapType("IPDAction").ShouldBe(typeof(IPDAction));
        TypeMapper.MapType("IPDMetadata").ShouldBe(typeof(IPDMetadata));
        TypeMapper.MapType("IPDOutputIntent").ShouldBe(typeof(IPDOutputIntent));
        TypeMapper.MapType("IPDStructTreeRoot").ShouldBe(typeof(IPDStructTreeRoot));
        TypeMapper.MapType("IPDStructElem").ShouldBe(typeof(IPDStructElem));
    }

    [Fact]
    public void TypeMapper_Should_Resolve_Xmp_And_Semantic_Interface_Targets()
    {
        TypeMapper.MapType("IXMPPackage").ShouldBe(typeof(IXMPPackage));
        TypeMapper.MapType("IMainXMPPackage").ShouldBe(typeof(IMainXMPPackage));
        TypeMapper.MapType("IXMPProperty").ShouldBe(typeof(IXMPProperty));
        TypeMapper.MapType("IPDFAIdentification").ShouldBe(typeof(IPDFAIdentification));
        TypeMapper.MapType("IPDFUAIdentification").ShouldBe(typeof(IPDFUAIdentification));
        TypeMapper.MapType("IExtensionSchemasContainer").ShouldBe(typeof(IExtensionSchemasContainer));
        TypeMapper.MapType("IExtensionSchemaDefinition").ShouldBe(typeof(IExtensionSchemaDefinition));
        TypeMapper.MapType("IExtensionSchemaProperty").ShouldBe(typeof(IExtensionSchemaProperty));
        TypeMapper.MapType("ISEL").ShouldBe(typeof(ISEL));
        TypeMapper.MapType("ISETable").ShouldBe(typeof(ISETable));
        TypeMapper.MapType("ISETableCell").ShouldBe(typeof(ISETableCell));
        TypeMapper.MapType("ISETOCI").ShouldBe(typeof(ISETOCI));
        TypeMapper.MapType("ISEHn").ShouldBe(typeof(ISEHn));
        TypeMapper.MapType("ISEContentItem").ShouldBe(typeof(ISEContentItem));
        TypeMapper.MapType("ISEGroupedContent").ShouldBe(typeof(ISEGroupedContent));
        TypeMapper.MapType("ISESimpleContentItem").ShouldBe(typeof(ISESimpleContentItem));
    }

    [Fact]
    public void TypeMapper_Should_Resolve_StructureAnalysis_Interface_Targets()
    {
        TypeMapper.MapType("ISAObject").ShouldBe(typeof(ISAObject));
        TypeMapper.MapType("ISAPDFDocument").ShouldBe(typeof(ISAPDFDocument));
        TypeMapper.MapType("ISAStructTreeRoot").ShouldBe(typeof(ISAStructTreeRoot));
        TypeMapper.MapType("ISAStructElem").ShouldBe(typeof(ISAStructElem));
        TypeMapper.MapType("ISAPage").ShouldBe(typeof(ISAPage));
        TypeMapper.MapType("ISAAnnotation").ShouldBe(typeof(ISAAnnotation));
        TypeMapper.MapType("ISALinkAnnotation").ShouldBe(typeof(ISALinkAnnotation));
        TypeMapper.MapType("ISATextChunk").ShouldBe(typeof(ISATextChunk));
        TypeMapper.MapType("ISATableBorder").ShouldBe(typeof(ISATableBorder));
        TypeMapper.MapType("ISATableBorderRow").ShouldBe(typeof(ISATableBorderRow));
        TypeMapper.MapType("ISATableBorderCell").ShouldBe(typeof(ISATableBorderCell));
        TypeMapper.MapType("ISAList").ShouldBe(typeof(ISAList));
        TypeMapper.MapType("ISAListItem").ShouldBe(typeof(ISAListItem));
    }
}
