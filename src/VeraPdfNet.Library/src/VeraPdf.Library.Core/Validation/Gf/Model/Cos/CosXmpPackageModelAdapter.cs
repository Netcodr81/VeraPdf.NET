using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using VeraPdf.Library.Xmp;
using VeraPdf.Model;
using VeraPdf.Model.BaseLayer;
using VeraPdf.Model.XMPLayer;
using VeraPdf.Parser;
using VeraPdf.Parser.Cos;
using VeraPdf.Parser.Filters;

namespace VeraPdf.Library.Validation.Gf.Model.Cos;

internal sealed class CosXmpPackageModelAdapter : GenericModelObject, IXMPPackage
{
    private const string ExtensionNamespace = "http://www.aiim.org/pdfa/ns/extension/";
    private const string SchemaNamespace = "http://www.aiim.org/pdfa/ns/schema#";
    private const string PropertyNamespace = "http://www.aiim.org/pdfa/ns/property#";

    private static readonly HashSet<string> SchemaDefinitionFields = new(StringComparer.Ordinal)
    {
        "namespaceURI",
        "prefix",
        "property",
        "schema",
        "valueType"
    };

    private const string TypeNamespace = "http://www.aiim.org/pdfa/ns/type#";
    private const string FieldNamespace = "http://www.aiim.org/pdfa/ns/field#";
    private const string DcNamespace = "http://purl.org/dc/elements/1.1/";
    private const string XmpNamespace = "http://ns.adobe.com/xap/1.0/";
    private const string XmpRightsNamespace = "http://ns.adobe.com/xap/1.0/rights/";
    private const string XmpMediaManagementNamespace = "http://ns.adobe.com/xap/1.0/mm/";
    private const string XmpBasicJobNamespace = "http://ns.adobe.com/xap/1.0/bj/";
    private const string XmpPagedTextNamespace = "http://ns.adobe.com/xap/1.0/t/pg/";
    private const string PdfNamespace = "http://ns.adobe.com/pdf/1.3/";
    private const string PhotoshopNamespace = "http://ns.adobe.com/photoshop/1.0/";
    private const string TiffNamespace = "http://ns.adobe.com/tiff/1.0/";
    private const string ExifNamespace = "http://ns.adobe.com/exif/1.0/";
    private const string ExifAuxNamespace = "http://ns.adobe.com/exif/1.0/aux/";
    private const string DimensionsNamespace = "http://ns.adobe.com/xap/1.0/sType/Dimensions#";
    private const string JobNamespace = "http://ns.adobe.com/xap/1.0/sType/Job#";
    private const string FontNamespace = "http://ns.adobe.com/xap/1.0/sType/Font#";
    private const string ResourceEventNamespace = "http://ns.adobe.com/xap/1.0/sType/ResourceEvent#";
    private const string ResourceReferenceNamespace = "http://ns.adobe.com/xap/1.0/sType/ResourceRef#";
    private const string VersionNamespace = "http://ns.adobe.com/xap/1.0/sType/Version#";
    private const string GraphicsNamespace = "http://ns.adobe.com/xap/1.0/g/";
    private const string ThumbnailNamespace = "http://ns.adobe.com/xap/1.0/g/img/";
    private const string CameraRawNamespace = "http://ns.adobe.com/camera-raw-settings/1.0/";
    private const string DynamicMediaNamespace = "http://ns.adobe.com/xmp/1.0/DynamicMedia/";
    private const string PdfaIdentificationNamespace = "http://www.aiim.org/pdfa/ns/id/";

    private static readonly HashSet<string> Exif2004OnlyProperties = new(StringComparer.Ordinal)
    {
        "MakerNote"
    };

    private static readonly HashSet<string> Exif2005OnlyProperties = new(StringComparer.Ordinal)
    {
        "ExifVersion",
        "FlashpixVersion"
    };

    private static readonly HashSet<string> ExifProfileDependentTypeProperties = new(StringComparer.Ordinal)
    {
        "GPSMeasureMode"
    };

    private static readonly HashSet<string> PdfaIdentification2005OnlyProperties = new(StringComparer.Ordinal)
    {
        "corr"
    };

    private static readonly HashSet<string> Xmp2005OnlyProperties = new(StringComparer.Ordinal)
    {
        "Label",
        "Rating"
    };

    private static readonly HashSet<string> XmpPagedText2005OnlyProperties = new(StringComparer.Ordinal)
    {
        "Fonts",
        "Colorants",
        "PlateNames"
    };

    private static readonly HashSet<string> ExifAux2005OnlyProperties = new(StringComparer.Ordinal)
    {
        "Lens",
        "SerialNumber"
    };

    private static readonly HashSet<string> PhotoshopCommonProfileDependentTypeProperties =
        new(StringComparer.Ordinal)
        {
            "SupplementalCategories"
        };

    private static readonly Regex BooleanValuePattern = new(
        @"\A(?:True|False)\z", RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

    private static readonly Regex IntegerValuePattern = new(
        @"\A[+-]?[0-9]+\z", RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

    private static readonly Regex RealValuePattern = new(
        @"\A[+-]?(?:[0-9]+\.?[0-9]*|[0-9]*\.?[0-9]+)\z",
        RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

    private static readonly Regex TimecodeValuePattern = new(
        @"\A(?:\d{2}(?::\d{2}){3}|\d{2}(?:;\d{2}){3})\z",
        RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

    private static readonly Regex GpsCoordinatePdfa1Pattern = new(
        @"\A\d{2},\d{2}[,.]\d{2}[NSEW]\z",
        RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

    private static readonly Regex GpsCoordinatePdfa23Pattern = new(
        @"\A\d{1,3},\d{1,2}(?:,\d{1,2}|\.\d+)[NSEW]\z",
        RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

    private static readonly IReadOnlyDictionary<string, Regex> ColorantRestrictedFieldPatterns =
        new Dictionary<string, Regex>(StringComparer.Ordinal)
        {
            ["mode"] = new(@"\A(?:CMYK|RGB|LAB)\z", RegexOptions.CultureInvariant | RegexOptions.NonBacktracking),
            ["type"] = new(@"\A(?:PROCESS|SPOT)\z", RegexOptions.CultureInvariant | RegexOptions.NonBacktracking),
            ["cyan"] = new(@"\A[+]?(?:\d{1,2}(?:\.\d*)?|\d{0,2}\.\d+|100(?:\.0*)?)\z", RegexOptions.CultureInvariant | RegexOptions.NonBacktracking),
            ["magenta"] = new(@"\A[+]?(?:\d{1,2}(?:\.\d*)?|\d{0,2}\.\d+|100(?:\.0*)?)\z", RegexOptions.CultureInvariant | RegexOptions.NonBacktracking),
            ["yellow"] = new(@"\A[+]?(?:\d{1,2}(?:\.\d*)?|\d{0,2}\.\d+|100(?:\.0*)?)\z", RegexOptions.CultureInvariant | RegexOptions.NonBacktracking),
            ["black"] = new(@"\A[+]?(?:\d{1,2}(?:\.\d*)?|\d{0,2}\.\d+|100(?:\.0*)?)\z", RegexOptions.CultureInvariant | RegexOptions.NonBacktracking),
            ["red"] = new(@"\A[+]?(?:[01]?[0-9]{1,2}|2[0-4][0-9]|25[0-5])\z", RegexOptions.CultureInvariant | RegexOptions.NonBacktracking),
            ["green"] = new(@"\A[+]?(?:[01]?[0-9]{1,2}|2[0-4][0-9]|25[0-5])\z", RegexOptions.CultureInvariant | RegexOptions.NonBacktracking),
            ["blue"] = new(@"\A[+]?(?:[01]?[0-9]{1,2}|2[0-4][0-9]|25[0-5])\z", RegexOptions.CultureInvariant | RegexOptions.NonBacktracking),
            ["L"] = new(@"\A[+]?(?:\d{1,2}(?:\.\d*)?|\d{0,2}\.\d+|100(?:\.0*)?)\z", RegexOptions.CultureInvariant | RegexOptions.NonBacktracking),
            ["A"] = new(@"\A(?:[+-]?[0]?[0-9]{1,2}|[+-]?1[01][0-9]|[+-]?12[0-7]|-128)\z", RegexOptions.CultureInvariant | RegexOptions.NonBacktracking),
            ["B"] = new(@"\A(?:[+-]?[0]?[0-9]{1,2}|[+-]?1[01][0-9]|[+-]?12[0-7]|-128)\z", RegexOptions.CultureInvariant | RegexOptions.NonBacktracking)
        };

    private static readonly Regex MimeTypeValuePattern = new(
        @"\A[-A-Za-z0-9_+.]+/[-A-Za-z0-9_+.]+\z",
        RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

    private static readonly IReadOnlyDictionary<(string NamespaceUri, string Name), string> CorePropertyTypes =
        new Dictionary<(string NamespaceUri, string Name), string>
        {
            [(DcNamespace, "contributor")] = "bag propername",
            [(DcNamespace, "coverage")] = "text",
            [(DcNamespace, "creator")] = "seq propername",
            [(DcNamespace, "date")] = "seq date",
            [(DcNamespace, "description")] = "lang alt",
            [(DcNamespace, "format")] = "mimetype",
            [(DcNamespace, "identifier")] = "text",
            [(DcNamespace, "language")] = "bag locale",
            [(DcNamespace, "publisher")] = "bag propername",
            [(DcNamespace, "relation")] = "bag text",
            [(DcNamespace, "rights")] = "lang alt",
            [(DcNamespace, "source")] = "text",
            [(DcNamespace, "subject")] = "bag text",
            [(DcNamespace, "title")] = "lang alt",
            [(DcNamespace, "type")] = "bag text",
            [(XmpNamespace, "Advisory")] = "bag text",
            [(XmpNamespace, "BaseURL")] = "url",
            [(XmpNamespace, "CreateDate")] = "date",
            [(XmpNamespace, "CreatorTool")] = "agentname",
            [(XmpNamespace, "Identifier")] = "bag text",
            [(XmpNamespace, "Label")] = "text",
            [(XmpNamespace, "MetadataDate")] = "date",
            [(XmpNamespace, "ModifyDate")] = "date",
            [(XmpNamespace, "Nickname")] = "text",
            [(XmpNamespace, "Rating")] = "real",
            [(XmpNamespace, "Thumbnails")] = "alt thumbnail",
            [(XmpRightsNamespace, "Certificate")] = "url",
            [(XmpRightsNamespace, "Marked")] = "boolean",
            [(XmpRightsNamespace, "Owner")] = "bag propername",
            [(XmpRightsNamespace, "UsageTerms")] = "lang alt",
            [(XmpRightsNamespace, "WebStatement")] = "url",
            [(XmpMediaManagementNamespace, "DerivedFrom")] = "resourceref",
            [(XmpMediaManagementNamespace, "DocumentID")] = "uri",
            [(XmpMediaManagementNamespace, "History")] = "seq resourceevent",
            [(XmpMediaManagementNamespace, "InstanceID")] = "uri",
            [(XmpMediaManagementNamespace, "ManagedFrom")] = "resourceref",
            [(XmpMediaManagementNamespace, "Manager")] = "agentname",
            [(XmpMediaManagementNamespace, "ManageTo")] = "uri",
            [(XmpMediaManagementNamespace, "ManageUI")] = "uri",
            [(XmpMediaManagementNamespace, "ManagerVariant")] = "text",
            [(XmpMediaManagementNamespace, "RenditionClass")] = "renditionclass",
            [(XmpMediaManagementNamespace, "RenditionParams")] = "text",
            [(XmpMediaManagementNamespace, "VersionID")] = "text",
            [(XmpMediaManagementNamespace, "Versions")] = "seq version",
            [(XmpMediaManagementNamespace, "LastURL")] = "url",
            [(XmpMediaManagementNamespace, "RenditionOf")] = "resourceref",
            [(XmpMediaManagementNamespace, "SaveID")] = "integer",
            [(XmpBasicJobNamespace, "JobRef")] = "bag job",
            [(XmpPagedTextNamespace, "MaxPageSize")] = "dimensions",
            [(XmpPagedTextNamespace, "NPages")] = "integer",
            [(XmpPagedTextNamespace, "Fonts")] = "bag font",
            [(XmpPagedTextNamespace, "Colorants")] = "seq colorant",
            [(XmpPagedTextNamespace, "PlateNames")] = "seq text",
            [(PdfaIdentificationNamespace, "part")] = "integer",
            [(PdfaIdentificationNamespace, "amd")] = "text",
            [(PdfaIdentificationNamespace, "conformance")] = "text",
            [(PdfaIdentificationNamespace, "corr")] = "text",
            [(PdfNamespace, "Keywords")] = "text",
            [(PdfNamespace, "PDFVersion")] = "text",
            [(PdfNamespace, "Producer")] = "agentname",
            [(PhotoshopNamespace, "AuthorsPosition")] = "text",
            [(PhotoshopNamespace, "CaptionWriter")] = "propername",
            [(PhotoshopNamespace, "Category")] = "text",
            [(PhotoshopNamespace, "City")] = "text",
            [(PhotoshopNamespace, "Country")] = "text",
            [(PhotoshopNamespace, "Credit")] = "text",
            [(PhotoshopNamespace, "DateCreated")] = "date",
            [(PhotoshopNamespace, "Headline")] = "text",
            [(PhotoshopNamespace, "Instructions")] = "text",
            [(PhotoshopNamespace, "Source")] = "text",
            [(PhotoshopNamespace, "State")] = "text",
            [(PhotoshopNamespace, "TransmissionReference")] = "text",
            [(PhotoshopNamespace, "Urgency")] = "integer",
            [(TiffNamespace, "ImageWidth")] = "integer",
            [(TiffNamespace, "ImageLength")] = "integer",
            [(TiffNamespace, "BitsPerSample")] = "seq integer",
            [(TiffNamespace, "SamplesPerPixel")] = "integer",
            [(TiffNamespace, "XResolution")] = "rational",
            [(TiffNamespace, "YResolution")] = "rational",
            [(TiffNamespace, "TransferFunction")] = "seq integer",
            [(TiffNamespace, "WhitePoint")] = "seq rational",
            [(TiffNamespace, "PrimaryChromaticities")] = "seq rational",
            [(TiffNamespace, "YCbCrCoefficients")] = "seq rational",
            [(TiffNamespace, "ReferenceBlackWhite")] = "seq rational",
            [(TiffNamespace, "DateTime")] = "date",
            [(TiffNamespace, "ImageDescription")] = "lang alt",
            [(TiffNamespace, "Make")] = "propername",
            [(TiffNamespace, "Model")] = "propername",
            [(TiffNamespace, "Software")] = "agentname",
            [(TiffNamespace, "Artist")] = "propername",
            [(TiffNamespace, "Copyright")] = "lang alt",
            [(TiffNamespace, "Compression")] = "integer",
            [(TiffNamespace, "PhotometricInterpretation")] = "integer",
            [(TiffNamespace, "Orientation")] = "integer",
            [(TiffNamespace, "PlanarConfiguration")] = "integer",
            [(TiffNamespace, "YCbCrPositioning")] = "integer",
            [(TiffNamespace, "ResolutionUnit")] = "integer",
            [(TiffNamespace, "YCbCrSubSampling")] = "seq integer",
            [(ExifNamespace, "CompressedBitsPerPixel")] = "rational",
            [(ExifNamespace, "PixelXDimension")] = "integer",
            [(ExifNamespace, "PixelYDimension")] = "integer",
            [(ExifNamespace, "UserComment")] = "lang alt",
            [(ExifNamespace, "RelatedSoundFile")] = "text",
            [(ExifNamespace, "DateTimeOriginal")] = "date",
            [(ExifNamespace, "DateTimeDigitized")] = "date",
            [(ExifNamespace, "ExposureTime")] = "rational",
            [(ExifNamespace, "FNumber")] = "rational",
            [(ExifNamespace, "SpectralSensitivity")] = "text",
            [(ExifNamespace, "ISOSpeedRatings")] = "seq integer",
            [(ExifNamespace, "OECF")] = "oecf/sfr",
            [(ExifNamespace, "ShutterSpeedValue")] = "rational",
            [(ExifNamespace, "ApertureValue")] = "rational",
            [(ExifNamespace, "BrightnessValue")] = "rational",
            [(ExifNamespace, "ExposureBiasValue")] = "rational",
            [(ExifNamespace, "MaxApertureValue")] = "rational",
            [(ExifNamespace, "SubjectDistance")] = "rational",
            [(ExifNamespace, "Flash")] = "flash",
            [(ExifNamespace, "FocalLength")] = "rational",
            [(ExifNamespace, "SubjectArea")] = "seq integer",
            [(ExifNamespace, "FlashEnergy")] = "rational",
            [(ExifNamespace, "SpatialFrequencyResponse")] = "oecf/sfr",
            [(ExifNamespace, "FocalPlaneXResolution")] = "rational",
            [(ExifNamespace, "FocalPlaneYResolution")] = "rational",
            [(ExifNamespace, "SubjectLocation")] = "seq integer",
            [(ExifNamespace, "ExposureIndex")] = "rational",
            [(ExifNamespace, "ColorSpace")] = "integer",
            [(ExifNamespace, "LightSource")] = "integer",
            [(ExifNamespace, "CFAPattern")] = "cfapattern",
            [(ExifNamespace, "DigitalZoomRatio")] = "rational",
            [(ExifNamespace, "FocalLengthIn35mmFilm")] = "integer",
            [(ExifNamespace, "DeviceSettingDescription")] = "devicesettings",
            [(ExifNamespace, "ImageUniqueID")] = "text",
            [(ExifNamespace, "GPSVersionID")] = "text",
            [(ExifNamespace, "GPSLatitude")] = "gpscoordinate",
            [(ExifNamespace, "GPSLongitude")] = "gpscoordinate",
            [(ExifNamespace, "GPSAltitude")] = "rational",
            [(ExifNamespace, "GPSTimeStamp")] = "date",
            [(ExifNamespace, "GPSSatellites")] = "text",
            [(ExifNamespace, "GPSDOP")] = "rational",
            [(ExifNamespace, "GPSSpeed")] = "rational",
            [(ExifNamespace, "GPSTrack")] = "rational",
            [(ExifNamespace, "GPSImgDirection")] = "rational",
            [(ExifNamespace, "GPSMapDatum")] = "text",
            [(ExifNamespace, "GPSDestLatitude")] = "gpscoordinate",
            [(ExifNamespace, "GPSDestLongitude")] = "gpscoordinate",
            [(ExifNamespace, "GPSDestBearing")] = "rational",
            [(ExifNamespace, "GPSDestDistance")] = "rational",
            [(ExifNamespace, "GPSProcessingMethod")] = "text",
            [(ExifNamespace, "GPSAreaInformation")] = "text",
            [(ExifNamespace, "ExposureProgram")] = "integer",
            [(ExifNamespace, "MeteringMode")] = "integer",
            [(ExifNamespace, "FocalPlaneResolutionUnit")] = "integer",
            [(ExifNamespace, "SensingMethod")] = "integer",
            [(ExifNamespace, "FileSource")] = "integer",
            [(ExifNamespace, "SceneType")] = "integer",
            [(ExifNamespace, "CustomRendered")] = "integer",
            [(ExifNamespace, "ExposureMode")] = "integer",
            [(ExifNamespace, "WhiteBalance")] = "integer",
            [(ExifNamespace, "SceneCaptureType")] = "integer",
            [(ExifNamespace, "GainControl")] = "integer",
            [(ExifNamespace, "Contrast")] = "integer",
            [(ExifNamespace, "Saturation")] = "integer",
            [(ExifNamespace, "Sharpness")] = "integer",
            [(ExifNamespace, "SubjectDistanceRange")] = "integer",
            [(ExifNamespace, "GPSAltitudeRef")] = "integer",
            [(ExifNamespace, "GPSStatus")] = "text",
            [(ExifNamespace, "GPSSpeedRef")] = "text",
            [(ExifNamespace, "GPSTrackRef")] = "text",
            [(ExifNamespace, "GPSImgDirectionRef")] = "text",
            [(ExifNamespace, "GPSDestBearingRef")] = "text",
            [(ExifNamespace, "GPSDestDistanceRef")] = "text",
            [(ExifNamespace, "GPSDifferential")] = "integer",
            [(ExifNamespace, "ComponentsConfiguration")] = "seq integer",
            [(ExifNamespace, "MakerNote")] = "text",
            [(ExifNamespace, "ExifVersion")] = "text",
            [(ExifNamespace, "FlashpixVersion")] = "text",
            [(CameraRawNamespace, "AutoBrightness")] = "boolean",
            [(CameraRawNamespace, "AutoContrast")] = "boolean",
            [(CameraRawNamespace, "AutoExposure")] = "boolean",
            [(CameraRawNamespace, "AutoShadows")] = "boolean",
            [(CameraRawNamespace, "BlueHue")] = "integer",
            [(CameraRawNamespace, "BlueSaturation")] = "integer",
            [(CameraRawNamespace, "Brightness")] = "integer",
            [(CameraRawNamespace, "CameraProfile")] = "text",
            [(CameraRawNamespace, "ChromaticAberrationB")] = "integer",
            [(CameraRawNamespace, "ChromaticAberrationR")] = "integer",
            [(CameraRawNamespace, "ColorNoiseReduction")] = "integer",
            [(CameraRawNamespace, "Contrast")] = "integer",
            [(CameraRawNamespace, "CropTop")] = "real",
            [(CameraRawNamespace, "CropLeft")] = "real",
            [(CameraRawNamespace, "CropBottom")] = "real",
            [(CameraRawNamespace, "CropRight")] = "real",
            [(CameraRawNamespace, "CropAngle")] = "real",
            [(CameraRawNamespace, "CropWidth")] = "real",
            [(CameraRawNamespace, "CropHeight")] = "real",
            [(CameraRawNamespace, "CropUnits")] = "integer",
            [(CameraRawNamespace, "Exposure")] = "real",
            [(CameraRawNamespace, "GreenHue")] = "integer",
            [(CameraRawNamespace, "GreenSaturation")] = "integer",
            [(CameraRawNamespace, "HasCrop")] = "boolean",
            [(CameraRawNamespace, "HasSettings")] = "boolean",
            [(CameraRawNamespace, "LuminanceSmoothing")] = "integer",
            [(CameraRawNamespace, "RawFileName")] = "text",
            [(CameraRawNamespace, "RedHue")] = "integer",
            [(CameraRawNamespace, "RedSaturation")] = "integer",
            [(CameraRawNamespace, "Saturation")] = "integer",
            [(CameraRawNamespace, "Shadows")] = "integer",
            [(CameraRawNamespace, "ShadowTint")] = "integer",
            [(CameraRawNamespace, "Sharpness")] = "integer",
            [(CameraRawNamespace, "Temperature")] = "integer",
            [(CameraRawNamespace, "Tint")] = "integer",
            [(CameraRawNamespace, "ToneCurveName")] = "text",
            [(CameraRawNamespace, "Version")] = "text",
            [(CameraRawNamespace, "VignetteAmount")] = "integer",
            [(CameraRawNamespace, "VignetteMidpoint")] = "integer",
            [(CameraRawNamespace, "WhiteBalance")] = "text",
            [(CameraRawNamespace, "ToneCurve")] = "text",
            [(DynamicMediaNamespace, "artist")] = "text",
            [(DynamicMediaNamespace, "album")] = "text",
            [(DynamicMediaNamespace, "trackNumber")] = "integer",
            [(DynamicMediaNamespace, "genre")] = "text",
            [(DynamicMediaNamespace, "copyright")] = "text",
            [(DynamicMediaNamespace, "releaseDate")] = "date",
            [(DynamicMediaNamespace, "composer")] = "text",
            [(DynamicMediaNamespace, "engineer")] = "text",
            [(DynamicMediaNamespace, "videoFrameRate")] = "text",
            [(DynamicMediaNamespace, "videoFrameSize")] = "dimensions",
            [(DynamicMediaNamespace, "videoPixelAspectRatio")] = "rational",
            [(DynamicMediaNamespace, "videoAlphaUnityIsTransparent")] = "boolean",
            [(DynamicMediaNamespace, "videoAlphaPremultipleColor")] = "colorant",
            [(DynamicMediaNamespace, "videoCompressor")] = "text",
            [(DynamicMediaNamespace, "audioSampleRate")] = "integer",
            [(DynamicMediaNamespace, "audioCompressor")] = "text",
            [(DynamicMediaNamespace, "speakerPlacement")] = "text",
            [(DynamicMediaNamespace, "fileDataRate")] = "rational",
            [(DynamicMediaNamespace, "tapeName")] = "text",
            [(DynamicMediaNamespace, "altTapeName")] = "text",
            [(DynamicMediaNamespace, "startTimecode")] = "timecode",
            [(DynamicMediaNamespace, "altTimecode")] = "timecode",
            [(DynamicMediaNamespace, "duration")] = "time",
            [(DynamicMediaNamespace, "scene")] = "text",
            [(DynamicMediaNamespace, "shotName")] = "text",
            [(DynamicMediaNamespace, "shotDate")] = "date",
            [(DynamicMediaNamespace, "shotLocation")] = "text",
            [(DynamicMediaNamespace, "logComment")] = "text",
            [(DynamicMediaNamespace, "markers")] = "seq marker",
            [(DynamicMediaNamespace, "contributedMedia")] = "bag media",
            [(DynamicMediaNamespace, "absPeakAudioFilePath")] = "uri",
            [(DynamicMediaNamespace, "relativePeakAudioFilePath")] = "uri",
            [(DynamicMediaNamespace, "videoModDate")] = "date",
            [(DynamicMediaNamespace, "audioModDate")] = "date",
            [(DynamicMediaNamespace, "metadataModDate")] = "date",
            [(DynamicMediaNamespace, "tempo")] = "real",
            [(DynamicMediaNamespace, "instrument")] = "text",
            [(DynamicMediaNamespace, "introTime")] = "time",
            [(DynamicMediaNamespace, "outCue")] = "time",
            [(DynamicMediaNamespace, "relativeTimestamp")] = "time",
            [(DynamicMediaNamespace, "loop")] = "boolean",
            [(DynamicMediaNamespace, "numberOfBeats")] = "real",
            [(DynamicMediaNamespace, "timeScaleParams")] = "timescalestretch",
            [(DynamicMediaNamespace, "resampleParams")] = "resamplestretch",
            [(DynamicMediaNamespace, "beatSpliceParams")] = "beatsplicestretch",
            [(DynamicMediaNamespace, "projectRef")] = "projectlink",
            [(DynamicMediaNamespace, "videoPixelDepth")] = "text",
            [(DynamicMediaNamespace, "videoColorSpace")] = "text",
            [(DynamicMediaNamespace, "videoAlphaMode")] = "text",
            [(DynamicMediaNamespace, "videoFieldOrder")] = "text",
            [(DynamicMediaNamespace, "pullDown")] = "text",
            [(DynamicMediaNamespace, "audioSampleType")] = "text",
            [(DynamicMediaNamespace, "audioChannelType")] = "text",
            [(DynamicMediaNamespace, "key")] = "text",
            [(DynamicMediaNamespace, "stretchMode")] = "text",
            [(DynamicMediaNamespace, "timeSignature")] = "text",
            [(DynamicMediaNamespace, "scaleType")] = "text",
            [(ExifAuxNamespace, "Lens")] = "text",
            [(ExifAuxNamespace, "SerialNumber")] = "text"
        };

    private static readonly Regex HeaderAttribute = new(
        """(?<name>[A-Za-z]+)\s*=\s*(?<quote>['"])(?<value>.*?)\k<quote>""",
        RegexOptions.CultureInvariant);

    private readonly XmpPacket? _packet;
    private readonly bool _serializationValid;
    private readonly string? _actualEncoding;
    private readonly bool _isMainMetadata;
    private readonly bool _mainPackageSchemasKnown;
    private readonly XmpNode? _mainPackageExtensionSchemas;

    private CosXmpPackageModelAdapter(
        XmpPacket? packet,
        bool serializationValid,
        string? actualEncoding,
        bool isMainMetadata = false,
        bool mainPackageSchemasKnown = false,
        XmpNode? mainPackageExtensionSchemas = null)
        : base("XMPPackage")
    {
        _packet = packet;
        _serializationValid = serializationValid;
        _actualEncoding = actualEncoding;
        _isMainMetadata = isMainMetadata;
        _mainPackageSchemasKnown = mainPackageSchemasKnown;
        _mainPackageExtensionSchemas = mainPackageExtensionSchemas;
    }

    public bool? GetisSerializationValid() => _serializationValid;
    public string? GetactualEncoding() => _actualEncoding;
    public string? Getbytes() => GetHeaderAttribute("bytes");
    public string? Getencoding() => GetHeaderAttribute("encoding");
    internal bool IsParsed => _packet is not null;
    internal XmpNode? ExtensionSchemas => GetExtensionSchemasNode();

    public override IReadOnlyList<IModelObject> GetLinkedObjects(string linkName)
    {
        if (linkName == "Properties")
        {
            var currentSchemas = GetExtensionSchemasNode();
            var mainSchemas = _isMainMetadata ? currentSchemas : _mainPackageExtensionSchemas;
            var mainSchemasKnown = _isMainMetadata ? _packet is not null : _mainPackageSchemasKnown;
            return _packet?.Properties
                .Where(node => node.NamespaceUri != ExtensionNamespace || node.Name != "schemas")
                .Select(node => (IModelObject) new XmpPropertyModelAdapter(
                    node,
                    currentSchemas,
                    mainSchemas,
                    mainSchemasKnown))
                .ToArray() ?? [];
        }

        if (linkName == "ExtensionSchemasContainers")
        {
            var schemas = GetExtensionSchemasNode();
            return schemas is null ? [] : [new XmpExtensionSchemasContainerModelAdapter(schemas)];
        }

        return base.GetLinkedObjects(linkName);
    }

    internal XmpNode? GetExtensionSchemasNode() =>
        _packet?.Properties.FirstOrDefault(node =>
            node.NamespaceUri == ExtensionNamespace && node.Name == "schemas");

    internal static CosXmpPackageModelAdapter FromStream(
        CosStream stream,
        PdfDocumentReader? reader)
        => FromStream(stream, reader, false, (false, null));

    internal static CosXmpPackageModelAdapter FromStream(
        CosStream stream,
        PdfDocumentReader? reader,
        bool isMainMetadata,
        (bool Known, XmpNode? Schemas) mainPackageSchemas)
    {
        ArgumentNullException.ThrowIfNull(stream);
        try
        {
            var bytes = new PdfStreamFilterService().Decode(ResolveDecodeReferences(stream, reader));
            var packet = XmpPacket.ParseDiscovered(new MemoryStream(bytes, false));
            var encoding = DetectEncoding(bytes);
            var text = encoding.Encoding.GetString(bytes);
            return new CosXmpPackageModelAdapter(
                packet,
                packet.PacketHeader is not null && HasExpectedPacketStart(text),
                encoding.Name,
                isMainMetadata,
                mainPackageSchemas.Known,
                mainPackageSchemas.Schemas);
        }
        catch (Exception exception) when (
            exception is IOException or XmpException or DecoderFallbackException)
        {
            Trace.TraceWarning($"Problems with parsing metadata. {exception.Message}");
            return new CosXmpPackageModelAdapter(
                null,
                false,
                null,
                isMainMetadata,
                mainPackageSchemas.Known,
                mainPackageSchemas.Schemas);
        }
    }

    private string? GetHeaderAttribute(string name)
    {
        if (_packet?.PacketHeader is not { } header)
        {
            return null;
        }

        foreach (Match match in HeaderAttribute.Matches(header))
        {
            if (match.Groups["name"].Value == name)
            {
                return match.Groups["value"].Value;
            }
        }

        return null;
    }

    private static bool HasExpectedPacketStart(string text)
    {
        const string prefix = "<?xpacket begin=";
        if (!text.StartsWith(prefix, StringComparison.Ordinal) || text.Length <= prefix.Length + 2)
        {
            return false;
        }

        var quote = text[prefix.Length];
        return quote is '"' or '\'' &&
               text[prefix.Length + 1] == '\uFEFF' &&
               text[prefix.Length + 2] == quote;
    }

    private static CosStream ResolveDecodeReferences(CosStream stream, PdfDocumentReader? reader)
    {
        if (reader is null)
        {
            return stream;
        }

        var dictionary = new CosDictionary();
        foreach (var (key, value) in stream.Dictionary.Entries)
        {
            dictionary.Set(key, key is "Filter" or "DecodeParms"
                ? ResolveDecodeValue(value, reader)
                : value);
        }

        return new CosStream(dictionary, stream.RawData);
    }

    private static CosValue ResolveDecodeValue(CosValue value, PdfDocumentReader reader)
    {
        value = reader.ResolveReference(value) ?? value;
        if (value is CosArray array)
        {
            var resolved = new CosArray();
            foreach (var item in array.Values)
            {
                var direct = reader.ResolveReference(item) ?? item;
                resolved.Add(direct is CosDictionary dictionary
                    ? ResolveDictionaryValues(dictionary, reader)
                    : direct);
            }

            return resolved;
        }

        return value is CosDictionary parameters
            ? ResolveDictionaryValues(parameters, reader)
            : value;
    }

    private static CosDictionary ResolveDictionaryValues(
        CosDictionary source,
        PdfDocumentReader reader)
    {
        var resolved = new CosDictionary();
        foreach (var (key, value) in source.Entries)
        {
            resolved.Set(key, reader.ResolveReference(value) ?? value);
        }

        return resolved;
    }

    private static (Encoding Encoding, string Name) DetectEncoding(byte[] bytes)
    {
        if (bytes.AsSpan().StartsWith(Encoding.UTF8.Preamble))
            return (new UTF8Encoding(false, true), "UTF-8");
        if (bytes.AsSpan().StartsWith(new byte[] {0x00, 0x00, 0xFE, 0xFF}))
            return (new UTF32Encoding(true, true, true), "UTF-32BE");
        if (bytes.AsSpan().StartsWith(new byte[] {0xFF, 0xFE, 0x00, 0x00}))
            return (new UTF32Encoding(false, true, true), "UTF-32");
        if (bytes.AsSpan().StartsWith(new byte[] {0xFE, 0xFF}))
            return (new UnicodeEncoding(true, true, true), "UTF-16BE");
        if (bytes.AsSpan().StartsWith(new byte[] {0xFF, 0xFE}))
            return (new UnicodeEncoding(false, true, true), "UTF-16");
        if (bytes.AsSpan().StartsWith(new byte[] {0x00, 0x00, 0x00, 0x3C}))
            return (new UTF32Encoding(true, false, true), "UTF-32BE");
        if (bytes.AsSpan().StartsWith(new byte[] {0x3C, 0x00, 0x00, 0x00}))
            return (new UTF32Encoding(false, false, true), "UTF-32");
        if (bytes.AsSpan().StartsWith(new byte[] {0x00, 0x3C, 0x00, 0x3F}))
            return (new UnicodeEncoding(true, false, true), "UTF-16BE");
        if (bytes.AsSpan().StartsWith(new byte[] {0x3C, 0x00, 0x3F, 0x00}))
            return (new UnicodeEncoding(false, false, true), "UTF-16");
        return (new UTF8Encoding(false, true), "UTF-8");
    }

    private sealed class XmpPropertyModelAdapter(
        XmpNode node,
        XmpNode? extensionSchemas,
        XmpNode? mainPackageExtensionSchemas,
        bool mainPackageSchemasKnown)
        : GenericModelObject("XMPProperty"), IXMPProperty
    {
        public override string? GetID() => $"{node.NamespaceUri} - {node.Prefix ?? "null"}:{node.Name}";
        public bool? GetisPredefinedInXMP2004() => IsCorePredefinedProperty(false);
        public bool? GetisPredefinedInXMP2005() => IsCorePredefinedProperty(true);
        public bool? GetisDefinedInCurrentPackage() => FindExtensionDeclaration().Found;

        public bool? GetisDefinedInMainPackage() =>
            mainPackageSchemasKnown
                ? FindExtensionDeclaration(mainPackageExtensionSchemas).Found
                : null;

        public bool? GetisValueTypeCorrect()
        {
            if (CorePropertyTypes.TryGetValue((node.NamespaceUri, node.Name), out var coreType))
            {
                if (node.NamespaceUri == TiffNamespace && node.Name == "YCbCrSubSampling")
                {
                    return IsClosedIntegerSequenceChoiceCorrect(node, ["2", "1"], ["2", "2"]);
                }

                if (node.NamespaceUri == ExifNamespace && node.Name == "ComponentsConfiguration")
                {
                    return IsClosedIntegerSequenceChoiceCorrect(
                        node,
                        ["4", "5", "6", "0"],
                        ["1", "2", "3", "0"]);
                }

                if (node.NamespaceUri == TiffNamespace && node.Name == "YCbCrPositioning")
                {
                    return IsClosedIntegerChoiceCorrect(node, "1", "2");
                }

                if (node.NamespaceUri == TiffNamespace && node.Name == "ResolutionUnit")
                {
                    return IsClosedIntegerChoiceCorrect(node, "2", "3");
                }

                if (node.NamespaceUri == TiffNamespace)
                {
                    var choices = node.Name switch
                    {
                        "Compression" => new[] { "1", "6" },
                        "PhotometricInterpretation" => new[] { "2", "6" },
                        "Orientation" => new[] { "1", "2", "3", "4", "5", "6", "7", "8" },
                        "PlanarConfiguration" => new[] { "1", "2" },
                        _ => null
                    };
                    if (choices is not null)
                    {
                        return IsClosedIntegerChoiceCorrect(node, choices);
                    }
                }

                if (node.NamespaceUri == ExifNamespace)
                {
                    var choices = node.Name switch
                    {
                        "ExposureProgram" => new[] { "0", "1", "2", "3", "4", "5", "6", "7", "8" },
                        "MeteringMode" => new[] { "0", "1", "2", "3", "4", "5", "6", "255" },
                        "FocalPlaneResolutionUnit" => new[] { "2", "3" },
                        "SensingMethod" => new[] { "1", "2", "3", "4", "5", "6", "7", "8" },
                        "FileSource" => new[] { "3" },
                        "SceneType" => new[] { "1" },
                        "CustomRendered" => new[] { "0", "1" },
                        "ExposureMode" => new[] { "0", "1", "2" },
                        _ => null
                    };
                    if (choices is not null)
                    {
                        return IsClosedIntegerChoiceCorrect(node, choices);
                    }
                }

                return IsBuiltInValueTypeCorrect(node, coreType);
            }

            var declaration = FindExtensionDeclaration();
            if (!declaration.Found || declaration.Type is null)
            {
                return null;
            }

            return IsBuiltInValueTypeCorrect(node, declaration.Type);
        }

        private static bool? IsClosedIntegerSequenceChoiceCorrect(
            XmpNode value,
            params string[][] choices)
        {
            var sequenceIsValid = IsBuiltInValueTypeCorrect(value, "seq integer");
            if (sequenceIsValid != true)
            {
                return sequenceIsValid;
            }

            var values = value.Children.Select(child => child.Value).ToArray();
            return choices.Any(choice => values.SequenceEqual(choice))
                ? true
                : null;
        }

        private static bool? IsClosedIntegerChoiceCorrect(XmpNode value, params string[] choices)
        {
            var integerIsValid = IsBuiltInValueTypeCorrect(value, "integer");
            if (integerIsValid != true)
            {
                return integerIsValid;
            }

            return choices.Contains(value.Value, StringComparer.Ordinal) ? true : null;
        }

        private static bool? IsBuiltInValueTypeCorrect(XmpNode value, string type)
        {
            if (TryGetArrayType(type, out var arrayKind, out var childType))
            {
                if (value.Kind != arrayKind)
                    return false;
                var hasUnknownChildType = false;
                foreach (var child in value.Children)
                {
                    var isChildCorrect = IsBuiltInValueTypeCorrect(child, childType);
                    if (isChildCorrect == false)
                        return false;
                    hasUnknownChildType |= isChildCorrect is null;
                }

                return hasUnknownChildType ? null : true;
            }

            return type.Trim().ToLowerInvariant() switch
            {
                "text" or "locale" or "propername" or "agentname" or "rational" or "renditionclass" =>
                    IsSimpleTextValue(value),
                "any" => true,
                "lang alt" => IsLanguageAlternativeValue(value),
                "boolean" => IsSimpleScalarValue(value, BooleanValuePattern),
                "integer" => IsSimpleScalarValue(value, IntegerValuePattern),
                "real" => IsSimpleScalarValue(value, RealValuePattern),
                "mimetype" => IsSimpleScalarValue(value, MimeTypeValuePattern),
                "uri" or "url" => IsSimpleTextValue(value),
                "gpscoordinate" => IsGpsCoordinateValue(value),
                "xpath" => IsXPathValue(value),
                "date" => IsDateValue(value),
                "dimensions" or "job" or "font" or "resourceevent" or "version" or "resourceref" or
                    "time" or "media" or "beatsplicestretch" or "timescalestretch" or
                    "resamplestretch" or "marker" or "projectlink" or "timecode" or "colorant" or
                    "flash" or "cfapattern" or "oecf/sfr" or "devicesettings" =>
                    IsStructuredValueCorrect(value, type.Trim().ToLowerInvariant()),
                "thumbnail" => IsThumbnailValue(value),
                _ => null
            };
        }

        private static bool? IsGpsCoordinateValue(XmpNode value)
        {
            if (!IsSimpleTextValue(value))
                return false;
            var text = value.Value ?? string.Empty;
            var validForPdfa1 = GpsCoordinatePdfa1Pattern.IsMatch(text);
            var validForPdfa23 = GpsCoordinatePdfa23Pattern.IsMatch(text);
            return validForPdfa1 == validForPdfa23 ? validForPdfa1 : null;
        }

        private static bool? IsThumbnailValue(XmpNode value)
        {
            if (value.Kind != XmpNodeKind.Structure)
                return false;
            var hasProfileDependentFormat = false;
            foreach (var child in value.Children)
            {
                if (child.NamespaceUri != ThumbnailNamespace)
                    return false;
                var childType = child.Name switch
                {
                    "height" or "width" => "integer",
                    "image" => "text",
                    _ => null
                };
                if (child.Name == "format")
                {
                    if (!IsSimpleTextValue(child) || child.Value != "JPEG")
                        hasProfileDependentFormat = true;
                    continue;
                }

                if (childType is null || IsBuiltInValueTypeCorrect(child, childType) != true)
                    return false;
            }

            return hasProfileDependentFormat ? null : true;
        }

        private static bool? IsStructuredValueCorrect(XmpNode value, string type)
        {
            if (value.Kind != XmpNodeKind.Structure)
                return false;
            var namespaceUri = type switch
            {
                "dimensions" => DimensionsNamespace,
                "job" => JobNamespace,
                "font" => FontNamespace,
                "resourceevent" => ResourceEventNamespace,
                "resourceref" => ResourceReferenceNamespace,
                "version" => VersionNamespace,
                "colorant" => GraphicsNamespace,
                "flash" => ExifNamespace,
                "cfapattern" => ExifNamespace,
                "oecf/sfr" => ExifNamespace,
                "devicesettings" => ExifNamespace,
                "time" or "media" or "beatsplicestretch" or "timescalestretch" or
                    "resamplestretch" or "marker" or "projectlink" or "timecode" =>
                    DynamicMediaNamespace,
                _ => null
            };
            var hasProfileDependentField = false;
            foreach (var child in value.Children)
            {
                if (child.NamespaceUri != namespaceUri)
                    return false;
                if ((type is "timescalestretch" or "resamplestretch") && child.Name == "quality")
                {
                    if (!IsSimpleTextValue(child))
                        return false;
                    if (child.Value is not ("High" or "Medium" or "Low"))
                        hasProfileDependentField = true;
                    continue;
                }

                if (type == "marker" && child.Name == "type")
                {
                    if (!IsSimpleTextValue(child))
                        return false;
                    if (child.Value is not ("Chapter" or "Cue" or "Beat" or "Track" or "Index"))
                        hasProfileDependentField = true;
                    continue;
                }

                if (type == "projectlink" && child.Name == "type")
                {
                    if (!IsSimpleTextValue(child))
                        return false;
                    if (child.Value is not ("movie" or "still" or "audio" or "custom"))
                        hasProfileDependentField = true;
                    continue;
                }

                if (type == "colorant" &&
                    ColorantRestrictedFieldPatterns.TryGetValue(child.Name, out var restrictionPattern))
                {
                    var colorantChildType = child.Name switch
                    {
                        "mode" or "type" => "text",
                        "cyan" or "magenta" or "yellow" or "black" or "L" => "real",
                        _ => "integer"
                    };
                    if (IsBuiltInValueTypeCorrect(child, colorantChildType) != true)
                        return false;
                    if (!restrictionPattern.IsMatch(child.Value ?? string.Empty))
                        hasProfileDependentField = true;
                    continue;
                }

                if (type == "timecode" && child.Name == "timeValue")
                {
                    if (!IsSimpleTextValue(child))
                        return false;
                    if (!TimecodeValuePattern.IsMatch(child.Value ?? string.Empty))
                        hasProfileDependentField = true;
                    continue;
                }

                if (type == "timecode" && child.Name == "timeFormat")
                {
                    if (!IsSimpleTextValue(child))
                        return false;
                    if (child.Value is not (
                        "24Timecode" or "25Timecode" or "2997DropTimecode" or "2997NonDropTimecode" or
                        "30Timecode" or "50Timecode" or "5994DropTimecode" or "5994NonDropTimecode" or
                        "60Timecode" or "23976Timecode"))
                        hasProfileDependentField = true;
                    continue;
                }

                if (type == "flash" && (child.Name is "Return" or "Mode"))
                {
                    if (!IsSimpleTextValue(child))
                        return false;
                    var isAllowed = child.Name == "Return"
                        ? child.Value is "0" or "2" or "3"
                        : child.Value is "0" or "1" or "2" or "3";
                    if (!isAllowed)
                        hasProfileDependentField = true;
                    continue;
                }

                var childType = (type, child.Name) switch
                {
                    ("dimensions", "w" or "h") => "real",
                    ("dimensions", "unit") => "text",
                    ("job", "name" or "id") => "text",
                    ("job", "url") => "url",
                    ("font", "fontName" or "fontFamily" or "fontFace" or "fontType" or
                        "versionString" or "fontFileName") => "text",
                    ("font", "composite") => "boolean",
                    ("font", "childFontFiles") => "seq text",
                    ("resourceevent", "action" or "parameters") => "text",
                    ("resourceevent", "instanceID") => "uri",
                    ("resourceevent", "softwareAgent") => "agentname",
                    ("resourceevent", "when") => "date",
                    ("version", "comments" or "version") => "text",
                    ("version", "event") => "resourceevent",
                    ("version", "modifyDate") => "date",
                    ("version", "modifier") => "propername",
                    ("colorant", "swatchName") => "text",
                    ("flash", "Fired" or "Function" or "RedEyeMode") => "boolean",
                    ("cfapattern", "Columns" or "Rows") => "integer",
                    ("cfapattern", "Values") => "seq integer",
                    ("oecf/sfr", "Columns" or "Rows") => "integer",
                    ("oecf/sfr", "Names") => "seq text",
                    ("oecf/sfr", "Values") => "seq rational",
                    ("devicesettings", "Columns" or "Rows") => "integer",
                    ("devicesettings", "Settings") => "seq text",
                    ("resourceref", "instanceID" or "documentID" or "manageTo" or "manageUI") => "uri",
                    ("resourceref", "versionID" or "renditionParams" or "managerVariant") => "text",
                    ("resourceref", "renditionClass") => "renditionclass",
                    ("resourceref", "manager") => "agentname",
                    ("time", "value") => "integer",
                    ("time", "scale") => "rational",
                    ("marker", "startTime" or "duration") => "any",
                    ("marker", "comment" or "name" or "target") => "text",
                    ("marker", "location") => "uri",
                    ("projectlink", "path") => "uri",
                    ("media", "path" or "webStatement") => "uri",
                    ("media", "track") => "text",
                    ("media", "startTime" or "duration") => "time",
                    ("media", "managed") => "boolean",
                    ("beatsplicestretch", "useFileBeatsMarker") => "boolean",
                    ("beatsplicestretch", "riseInDecibel") => "real",
                    ("beatsplicestretch", "riseInTimeDuration") => "time",
                    ("timescalestretch", "frameSize" or "frameOverlappingPercentage") => "real",
                    _ => null
                };
                if (childType is null || IsBuiltInValueTypeCorrect(child, childType) != true)
                    return false;
            }

            return hasProfileDependentField ? null : true;
        }

        private static bool TryGetArrayType(string type, out XmpNodeKind arrayKind, out string childType)
        {
            var normalizedType = type.Trim().ToLowerInvariant();
            var separator = normalizedType.IndexOf(' ');
            var arrayType = separator < 0 ? normalizedType : normalizedType[..separator];
            switch (arrayType)
            {
                case "bag":
                    arrayKind = XmpNodeKind.UnorderedArray;
                    break;
                case "seq":
                    arrayKind = XmpNodeKind.OrderedArray;
                    break;
                case "alt":
                    arrayKind = XmpNodeKind.AlternativeArray;
                    break;
                default:
                    arrayKind = default;
                    childType = "";
                    return false;
            }

            childType = separator < 0 ? "text" : normalizedType[(separator + 1)..].Trim();
            if (childType.Length == 0)
                childType = "text";
            return true;
        }

        private static bool IsSimpleTextValue(XmpNode value) =>
            value.Kind is XmpNodeKind.Literal or XmpNodeKind.Uri;

        private static bool IsSimpleScalarValue(XmpNode value, Regex pattern) =>
            IsSimpleTextValue(value) && value.Value is { } text && pattern.IsMatch(text);

        private static bool IsXPathValue(XmpNode value)
        {
            if (!IsSimpleTextValue(value) || value.Value is not { } expression)
                return false;
            try
            {
                XPathExpression.Compile(expression);
                return true;
            }
            catch (XPathException)
            {
                return false;
            }
        }

        private static bool IsDateValue(XmpNode value)
        {
            if (!IsSimpleTextValue(value) || value.Value is not { } date)
                return false;
            try
            {
                XmpScalarConverter.ToDate(date);
                return true;
            }
            catch (XmpException)
            {
                return false;
            }
        }

        private static bool IsLanguageAlternativeValue(XmpNode value) =>
            value.Kind == XmpNodeKind.AlternativeArray &&
            (value.Children.Count == 0 || value.Children.Any(child => child.Language is not null));

        public string? GetpredefinedType()
        {
            if (CorePropertyTypes.TryGetValue((node.NamespaceUri, node.Name), out var coreType))
            {
                return coreType;
            }

            var declaration = FindExtensionDeclaration();
            return declaration.Found ? declaration.Type : null;
        }

        private bool? IsCorePredefinedProperty(bool pdfa2Or3)
        {
            if (node.NamespaceUri == CameraRawNamespace)
            {
                return pdfa2Or3
                    ? CorePropertyTypes.ContainsKey((node.NamespaceUri, node.Name))
                    : false;
            }

            if (node.NamespaceUri == DynamicMediaNamespace)
            {
                return pdfa2Or3
                    ? CorePropertyTypes.ContainsKey((node.NamespaceUri, node.Name))
                    : false;
            }

            if (node.NamespaceUri == ExifNamespace)
            {
                if (pdfa2Or3 && Exif2004OnlyProperties.Contains(node.Name) ||
                    !pdfa2Or3 && Exif2005OnlyProperties.Contains(node.Name))
                {
                    return false;
                }

                return CorePropertyTypes.ContainsKey((node.NamespaceUri, node.Name)) ||
                       ExifProfileDependentTypeProperties.Contains(node.Name);
            }

            if (node.NamespaceUri == PdfaIdentificationNamespace &&
                PdfaIdentification2005OnlyProperties.Contains(node.Name))
            {
                return pdfa2Or3;
            }

            if (node.NamespaceUri == XmpNamespace && Xmp2005OnlyProperties.Contains(node.Name))
            {
                return pdfa2Or3;
            }

            if (node.NamespaceUri == XmpPagedTextNamespace &&
                XmpPagedText2005OnlyProperties.Contains(node.Name))
            {
                return pdfa2Or3;
            }

            if (node.NamespaceUri == ExifAuxNamespace &&
                ExifAux2005OnlyProperties.Contains(node.Name))
            {
                return pdfa2Or3;
            }

            if (node.NamespaceUri == PhotoshopNamespace &&
                PhotoshopCommonProfileDependentTypeProperties.Contains(node.Name))
            {
                return true;
            }

            return node.NamespaceUri is DcNamespace or XmpNamespace or XmpRightsNamespace or
                XmpMediaManagementNamespace or XmpBasicJobNamespace or XmpPagedTextNamespace or
                ExifAuxNamespace or PdfaIdentificationNamespace or PdfNamespace or
                PhotoshopNamespace or TiffNamespace
                ? CorePropertyTypes.ContainsKey((node.NamespaceUri, node.Name))
                : null;
        }

        private (bool Found, string? Type) FindExtensionDeclaration() =>
            FindExtensionDeclaration(extensionSchemas);

        private (bool Found, string? Type) FindExtensionDeclaration(XmpNode? schemas)
        {
            if (schemas?.Kind is not (XmpNodeKind.UnorderedArray or
                XmpNodeKind.OrderedArray or XmpNodeKind.AlternativeArray))
            {
                return (false, null);
            }

            foreach (var schema in schemas.Children)
            {
                var namespaceUri = schema.Children.FirstOrDefault(child =>
                    child.NamespaceUri == SchemaNamespace && child.Name == "namespaceURI")?.Value;
                if (namespaceUri != node.NamespaceUri)
                {
                    continue;
                }

                var properties = schema.Children.FirstOrDefault(child =>
                    child.NamespaceUri == SchemaNamespace && child.Name == "property");
                if (properties?.Kind is not (XmpNodeKind.UnorderedArray or
                    XmpNodeKind.OrderedArray or XmpNodeKind.AlternativeArray))
                {
                    continue;
                }

                foreach (var property in properties.Children)
                {
                    var name = property.Children.FirstOrDefault(child =>
                        child.NamespaceUri == PropertyNamespace &&
                        child.Name == "name")?.Value;
                    if (name == node.Name)
                    {
                        var type = property.Children.FirstOrDefault(child =>
                            child.NamespaceUri == PropertyNamespace &&
                            child.Name == "valueType")?.Value;
                        return (true, type);
                    }
                }
            }

            return (false, null);
        }
    }

    private sealed class XmpExtensionSchemasContainerModelAdapter(XmpNode schemas)
        : GenericModelObject("ExtensionSchemasContainer"), IExtensionSchemasContainer
    {
        public string? Getprefix() => schemas.Prefix;
        public bool? GetisValidBag() => schemas.Kind == XmpNodeKind.UnorderedArray;

        public override IReadOnlyList<IModelObject> GetLinkedObjects(string linkName)
        {
            if (linkName == "ExtensionSchemaDefinitions")
            {
                if (schemas.Kind is not (XmpNodeKind.UnorderedArray or
                    XmpNodeKind.OrderedArray or XmpNodeKind.AlternativeArray))
                {
                    return [];
                }

                return schemas.Children
                    .Select(node => (IModelObject) new XmpExtensionSchemaDefinitionModelAdapter(node))
                    .ToArray();
            }

            return base.GetLinkedObjects(linkName);
        }
    }

    private sealed class XmpExtensionSchemaDefinitionModelAdapter(XmpNode node)
        : GenericModelObject("ExtensionSchemaDefinition"), IExtensionSchemaDefinition
    {
        public bool? GetcontainsUndefinedFields() =>
            node.Children.Any(child =>
                child.NamespaceUri != SchemaNamespace || !SchemaDefinitionFields.Contains(child.Name));

        public string? GetundefinedFields() => string.Join(",",
            node.Children
                .Where(child =>
                    child.NamespaceUri != SchemaNamespace || !SchemaDefinitionFields.Contains(child.Name))
                .Select(child => $"{child.Name}({child.NamespaceUri})"));

        public bool? GetisNamespaceURIValidURI() => IsSimple("namespaceURI");
        public bool? GetisPrefixValidText() => IsSimple("prefix");
        public bool? GetisPropertyValidSeq() => IsArray("property", XmpNodeKind.OrderedArray);
        public bool? GetisSchemaValidText() => IsSimple("schema");
        public bool? GetisValueTypeValidSeq() => IsArray("valueType", XmpNodeKind.OrderedArray);
        public string? GetnamespaceURIPrefix() => GetField("namespaceURI")?.Prefix;
        public string? GetprefixPrefix() => GetField("prefix")?.Prefix;
        public string? GetpropertyPrefix() => GetField("property")?.Prefix;
        public string? GetschemaPrefix() => GetField("schema")?.Prefix;
        public string? GetvalueTypePrefix() => GetField("valueType")?.Prefix;

        public override IReadOnlyList<IModelObject> GetLinkedObjects(string linkName)
        {
            var childName = linkName switch
            {
                "ExtensionSchemaProperties" => "property",
                "ExtensionSchemaValueTypes" => "valueType",
                _ => null
            };
            if (childName is null)
            {
                return base.GetLinkedObjects(linkName);
            }

            var collection = GetField(childName);
            if (collection?.Kind is not (XmpNodeKind.UnorderedArray or
                    XmpNodeKind.OrderedArray or XmpNodeKind.AlternativeArray) ||
                collection.Children.Count == 0)
            {
                return [];
            }

            if (childName == "property")
            {
                return collection.Children
                    .Select(child => (IModelObject) new XmpExtensionSchemaPropertyModelAdapter(child))
                    .ToArray();
            }

            return collection.Children
                .Select(child => (IModelObject) new XmpExtensionSchemaValueTypeModelAdapter(child))
                .ToArray();
        }

        private XmpNode? GetField(string name) =>
            node.Children.FirstOrDefault(child =>
                child.NamespaceUri == SchemaNamespace && child.Name == name);

        private bool IsSimple(string name)
        {
            var field = GetField(name);
            return field is null || field.Kind is XmpNodeKind.Literal or XmpNodeKind.Uri;
        }

        private bool IsArray(string name, XmpNodeKind expectedKind)
        {
            var field = GetField(name);
            return field is null || field.Kind == expectedKind;
        }
    }

    private sealed class XmpExtensionSchemaPropertyModelAdapter(XmpNode node)
        : GenericModelObject("ExtensionSchemaProperty"), IExtensionSchemaProperty
    {
        private static readonly HashSet<string> ValidFields = new(StringComparer.Ordinal)
        {
            "category",
            "description",
            "name",
            "valueType"
        };

        public bool? GetcontainsUndefinedFields() =>
            node.Children.Any(child =>
                child.NamespaceUri != PropertyNamespace || !ValidFields.Contains(child.Name));

        public string? GetundefinedFields() => string.Join(",",
            node.Children
                .Where(child =>
                    child.NamespaceUri != PropertyNamespace || !ValidFields.Contains(child.Name))
                .Select(child => $"{child.Name}({child.NamespaceUri})"));

        public string? Getcategory() => GetField("category")?.Value;
        public bool? GetisCategoryValidText() => IsSimple("category");
        public bool? GetisDescriptionValidText() => IsSimple("description");
        public bool? GetisNameValidText() => IsSimple("name");
        public bool? GetisValueTypeValidText() => IsSimple("valueType");

        public bool? GetisValueTypeDefined()
        {
            var valueType = GetField("valueType");
            if (valueType is null)
            {
                return true;
            }

            return valueType.Kind is XmpNodeKind.Literal or XmpNodeKind.Uri ? null : false;
        }

        public string? GetcategoryPrefix() => GetField("category")?.Prefix;
        public string? GetdescriptionPrefix() => GetField("description")?.Prefix;
        public string? GetnamePrefix() => GetField("name")?.Prefix;
        public string? GetvalueTypePrefix() => GetField("valueType")?.Prefix;

        private XmpNode? GetField(string name) =>
            node.Children.FirstOrDefault(child =>
                child.NamespaceUri == PropertyNamespace && child.Name == name);

        private bool IsSimple(string name)
        {
            var field = GetField(name);
            return field is null || field.Kind is XmpNodeKind.Literal or XmpNodeKind.Uri;
        }
    }

    private sealed class XmpExtensionSchemaValueTypeModelAdapter(XmpNode node)
        : GenericModelObject("ExtensionSchemaValueType"), IExtensionSchemaValueType
    {
        private static readonly HashSet<string> ValidFields = new(StringComparer.Ordinal)
        {
            "description",
            "field",
            "namespaceURI",
            "prefix",
            "type"
        };

        public bool? GetcontainsUndefinedFields() =>
            node.Children.Any(child =>
                child.NamespaceUri != TypeNamespace || !ValidFields.Contains(child.Name));

        public string? GetundefinedFields() => string.Join(",",
            node.Children
                .Where(child =>
                    child.NamespaceUri != TypeNamespace || !ValidFields.Contains(child.Name))
                .Select(child => $"{child.Name}({child.NamespaceUri})"));

        public bool? GetisDescriptionValidText() => IsSimple("description");

        public bool? GetisFieldValidSeq()
        {
            var field = GetField("field");
            return field is null || field.Kind == XmpNodeKind.OrderedArray;
        }

        public bool? GetisNamespaceURIValidURI() => IsSimple("namespaceURI");
        public bool? GetisPrefixValidText() => IsSimple("prefix");
        public bool? GetisTypeValidText() => IsSimple("type");
        public string? GetdescriptionPrefix() => GetField("description")?.Prefix;
        public string? GetfieldPrefix() => GetField("field")?.Prefix;
        public string? GetnamespaceURIPrefix() => GetField("namespaceURI")?.Prefix;
        public string? GetprefixPrefix() => GetField("prefix")?.Prefix;
        public string? GettypePrefix() => GetField("type")?.Prefix;

        public override IReadOnlyList<IModelObject> GetLinkedObjects(string linkName)
        {
            if (linkName != "ExtensionSchemaFields")
            {
                return base.GetLinkedObjects(linkName);
            }

            var fields = GetField("field");
            if (fields?.Kind is not (XmpNodeKind.UnorderedArray or
                XmpNodeKind.OrderedArray or XmpNodeKind.AlternativeArray))
            {
                return [];
            }

            return fields.Children
                .Select(child => (IModelObject) new XmpExtensionSchemaFieldModelAdapter(child))
                .ToArray();
        }

        private XmpNode? GetField(string name) =>
            node.Children.FirstOrDefault(child =>
                child.NamespaceUri == TypeNamespace && child.Name == name);

        private bool IsSimple(string name)
        {
            var field = GetField(name);
            return field is null || field.Kind is XmpNodeKind.Literal or XmpNodeKind.Uri;
        }
    }

    private sealed class XmpExtensionSchemaFieldModelAdapter(XmpNode node)
        : GenericModelObject("ExtensionSchemaField"), IExtensionSchemaField
    {
        private static readonly HashSet<string> ValidFields = new(StringComparer.Ordinal)
        {
            "description",
            "name",
            "valueType"
        };

        public bool? GetcontainsUndefinedFields() =>
            node.Children.Any(child =>
                child.NamespaceUri != FieldNamespace || !ValidFields.Contains(child.Name));

        public string? GetundefinedFields() => string.Join(",",
            node.Children
                .Where(child =>
                    child.NamespaceUri != FieldNamespace || !ValidFields.Contains(child.Name))
                .Select(child => $"{child.Name}({child.NamespaceUri})"));

        public bool? GetisDescriptionValidText() => IsSimple("description");
        public bool? GetisNameValidText() => IsSimple("name");
        public bool? GetisValueTypeValidText() => IsSimple("valueType");

        public bool? GetisValueTypeDefined()
        {
            var valueType = GetField("valueType");
            if (valueType is null)
            {
                return true;
            }

            return valueType.Kind is XmpNodeKind.Literal or XmpNodeKind.Uri ? null : false;
        }

        public string? GetdescriptionPrefix() => GetField("description")?.Prefix;
        public string? GetnamePrefix() => GetField("name")?.Prefix;
        public string? GetvalueTypePrefix() => GetField("valueType")?.Prefix;

        private XmpNode? GetField(string name) =>
            node.Children.FirstOrDefault(child =>
                child.NamespaceUri == FieldNamespace && child.Name == name);

        private bool IsSimple(string name)
        {
            var field = GetField(name);
            return field is null || field.Kind is XmpNodeKind.Literal or XmpNodeKind.Uri;
        }
    }
}