using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace VeraPdf.Library.Validation;

/// <summary>Reads and writes the persisted fields of a validator configuration.</summary>
public static class ValidatorConfigurationXml
{
    /// <summary>
    /// Reads an unqualified validatorConfig element, leaving the stream open.
    /// Password and NonPdfExtension are not persisted by the Java implementation.
    /// </summary>
    public static ValidatorConfiguration Read(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var reader = XmlReader.Create(stream, new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            CloseInput = false
        });
        var root = XDocument.Load(reader).Root;
        if (root?.Name != XName.Get("validatorConfig"))
            throw new InvalidDataException("Expected an unqualified validatorConfig element.");

        var defaults = new ValidatorConfiguration();
        return new ValidatorConfiguration
        {
            Flavour = (string?)root.Attribute("flavour") ?? defaults.Flavour,
            DefaultFlavour = (string?)root.Attribute("defaultFlavour") ?? defaults.DefaultFlavour,
            RecordPasses = (bool?)root.Attribute("recordPasses") ?? defaults.RecordPasses,
            MaxFails = (int?)root.Attribute("maxFails") ?? defaults.MaxFails,
            Debug = (bool?)root.Attribute("debug") ?? defaults.Debug,
            LogsEnabled = (bool?)root.Attribute("isLogsEnabled") ?? defaults.LogsEnabled,
            LoggingLevel = (string?)root.Attribute("loggingLevel") ?? defaults.LoggingLevel,
            MaxDisplayedFailedChecks = (int?)root.Attribute("maxNumberOfDisplayedFailedChecks") ?? defaults.MaxDisplayedFailedChecks,
            ShowErrorMessages = (bool?)root.Attribute("showErrorMessages") ?? defaults.ShowErrorMessages,
            ShowProgress = (bool?)root.Attribute("showProgress") ?? defaults.ShowProgress
        };
    }

    /// <summary>
    /// Writes UTF-8 XML without closing the stream. Password and NonPdfExtension
    /// are intentionally excluded, matching the Java XML mapping.
    /// </summary>
    public static void Write(ValidatorConfiguration configuration, Stream stream, bool indent = false)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(stream);
        using var writer = XmlWriter.Create(stream, new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(false),
            Indent = indent,
            CloseOutput = false,
            NewLineHandling = NewLineHandling.Entitize
        });
        writer.WriteStartDocument();
        writer.WriteStartElement("validatorConfig");
        writer.WriteAttributeString("flavour", configuration.Flavour);
        writer.WriteAttributeString("defaultFlavour", configuration.DefaultFlavour);
        writer.WriteAttributeString("recordPasses", XmlConvert.ToString(configuration.RecordPasses));
        writer.WriteAttributeString("maxFails", XmlConvert.ToString(configuration.MaxFails));
        writer.WriteAttributeString("debug", XmlConvert.ToString(configuration.Debug));
        writer.WriteAttributeString("isLogsEnabled", XmlConvert.ToString(configuration.LogsEnabled));
        writer.WriteAttributeString("loggingLevel", configuration.LoggingLevel);
        writer.WriteAttributeString("maxNumberOfDisplayedFailedChecks", XmlConvert.ToString(configuration.MaxDisplayedFailedChecks));
        writer.WriteAttributeString("showErrorMessages", XmlConvert.ToString(configuration.ShowErrorMessages));
        writer.WriteAttributeString("showProgress", XmlConvert.ToString(configuration.ShowProgress));
        writer.WriteEndElement();
        writer.WriteEndDocument();
    }
}
