using VeraPdf.Library.Validation;
using System.Text;
using System.Xml;

namespace VeraPdf.Library.Tests;

public class ValidatorConfigurationTests
{
    [Fact]
    public void DefaultsMatchReferenceBuilder()
    {
        var config = new ValidatorConfiguration();
        Assert.Equal("NO_FLAVOUR", config.Flavour);
        Assert.Equal("PDFA_1_B", config.DefaultFlavour);
        Assert.False(config.RecordPasses);
        Assert.Equal(-1, config.MaxFails);
        Assert.False(config.Debug);
        Assert.False(config.LogsEnabled);
        Assert.Equal("WARNING", config.LoggingLevel);
        Assert.Equal(100, config.MaxDisplayedFailedChecks);
        Assert.True(config.ShowErrorMessages);
        Assert.Equal("", config.Password);
        Assert.False(config.ShowProgress);
        Assert.False(config.NonPdfExtension);
    }

    [Fact]
    public void SupportsExplicitSettingsWithoutLoggingPassword()
    {
        var config = new ValidatorConfiguration
        {
            Flavour = "PDFUA_2",
            DefaultFlavour = "PDFA_2_B",
            RecordPasses = true,
            MaxFails = 3,
            Debug = true,
            LogsEnabled = true,
            LoggingLevel = "FINE",
            MaxDisplayedFailedChecks = -1,
            ShowErrorMessages = false,
            Password = "example-secret",
            ShowProgress = true,
            NonPdfExtension = true
        };
        Assert.Equal(3, config.MaxFails);
        Assert.Equal(-1, config.MaxDisplayedFailedChecks);
        Assert.Equal("example-secret", config.Password);
        Assert.DoesNotContain(config.Password, config.ToString());
    }

    [Fact]
    public void XmlRoundTripPreservesPersistedValuesWithoutLeakingPassword()
    {
        var original = new ValidatorConfiguration
        {
            Flavour = "PDFUA_2", DefaultFlavour = "PDFA_2_B", RecordPasses = true, MaxFails = 5,
            Debug = true, LogsEnabled = true, LoggingLevel = "FINE", MaxDisplayedFailedChecks = -1,
            ShowErrorMessages = false, Password = "example-secret", ShowProgress = true, NonPdfExtension = true
        };
        using var stream = new MemoryStream();
        ValidatorConfigurationXml.Write(original, stream);
        Assert.True(stream.CanWrite);
        var xml = Encoding.UTF8.GetString(stream.ToArray());
        Assert.DoesNotContain("example-secret", xml);
        Assert.DoesNotContain("nonPDFExtension", xml);
        stream.Position = 0;
        var copy = ValidatorConfigurationXml.Read(stream);
        Assert.True(stream.CanRead);
        Assert.Equal(original.Flavour, copy.Flavour);
        Assert.Equal(original.DefaultFlavour, copy.DefaultFlavour);
        Assert.Equal(original.RecordPasses, copy.RecordPasses);
        Assert.Equal(original.MaxFails, copy.MaxFails);
        Assert.Equal(original.Debug, copy.Debug);
        Assert.Equal(original.LogsEnabled, copy.LogsEnabled);
        Assert.Equal(original.LoggingLevel, copy.LoggingLevel);
        Assert.Equal(original.MaxDisplayedFailedChecks, copy.MaxDisplayedFailedChecks);
        Assert.Equal(original.ShowErrorMessages, copy.ShowErrorMessages);
        Assert.Equal(original.ShowProgress, copy.ShowProgress);
        Assert.Equal("", copy.Password);
        Assert.False(copy.NonPdfExtension);
    }

    [Fact]
    public void XmlDefaultsAndInvalidInputAreExplicit()
    {
        using var empty = new MemoryStream(Encoding.UTF8.GetBytes("<validatorConfig />"));
        var defaults = ValidatorConfigurationXml.Read(empty);
        Assert.Equal(-1, defaults.MaxFails);
        Assert.Equal(100, defaults.MaxDisplayedFailedChecks);
        Assert.True(defaults.ShowErrorMessages);
        using var invalid = new MemoryStream(Encoding.UTF8.GetBytes("<validatorConfig maxFails=\"no\" />"));
        Assert.Throws<FormatException>(() => ValidatorConfigurationXml.Read(invalid));
        using var dtd = new MemoryStream(Encoding.UTF8.GetBytes("<!DOCTYPE validatorConfig><validatorConfig />"));
        Assert.Throws<XmlException>(() => ValidatorConfigurationXml.Read(dtd));
        using var root = new MemoryStream(Encoding.UTF8.GetBytes("<other />"));
        Assert.Throws<InvalidDataException>(() => ValidatorConfigurationXml.Read(root));
    }
}
