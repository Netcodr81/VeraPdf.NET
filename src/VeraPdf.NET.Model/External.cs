namespace VeraPdf.NET.Model;

/// <summary>
/// Parent type for all external objects embedded into the PDF document.
/// </summary>
public class External : PDFObject
{
}

/// <summary>
/// Embedded ICC profile.
/// </summary>
public class ICCProfile : External
{
    /// <summary>
    /// Value of the N key in the ICC profile stream dictionary.
    /// </summary>
    public int N { get; set; }

    /// <summary>
    /// Device class identifier of the profile.
    /// </summary>
    public string? DeviceClass { get; set; }

    /// <summary>
    /// Color space identifier of the profile.
    /// </summary>
    public string? ColorSpace { get; set; }

    /// <summary>
    /// Version of the ICC profile.
    /// </summary>
    public decimal Version { get; set; }

    /// <summary>
    /// True if the profile satisfies the corresponding ICC specification.
    /// </summary>
    public bool IsValid { get; set; }
}

/// <summary>
/// Embedded ICC profile used for ICCBased color spaces.
/// </summary>
public class ICCInputProfile : ICCProfile
{
}

/// <summary>
/// Embedded ICC profile used as a destination profile in output intent.
/// </summary>
public class ICCOutputProfile : ICCProfile
{
    /// <summary>
    /// Subtype of the output profile.
    /// </summary>
    public string? S { get; set; }
}

/// <summary>
/// Embedded font program.
/// </summary>
public class FontProgram : External
{
}

/// <summary>
/// TrueType font program embedded into the PDF document.
/// </summary>
public class TrueTypeFontProgram : FontProgram
{
    /// <summary>
    /// True if corresponding PDF font is marked symbolic.
    /// </summary>
    public bool IsSymbolic { get; set; }

    /// <summary>
    /// Number of cmap subtables in the font program.
    /// </summary>
    public int NrCmaps { get; set; }

    /// <summary>
    /// True if Microsoft Symbol cmap (3,0) is present.
    /// </summary>
    public bool Cmap30Present { get; set; }

    /// <summary>
    /// True if Microsoft Unicode cmap (3,1) is present.
    /// </summary>
    public bool Cmap31Present { get; set; }

    /// <summary>
    /// True if Macintosh Roman cmap (1,0) is present.
    /// </summary>
    public bool Cmap10Present { get; set; }
}

/// <summary>
/// Embedded CMap file.
/// </summary>
public class CMapFile : External
{
    /// <summary>
    /// Value of WMode entry in the embedded CMap file.
    /// </summary>
    public int WMode { get; set; }

    /// <summary>
    /// Value of WMode entry in the parent CMap dictionary.
    /// </summary>
    public int DictWMode { get; set; }

    /// <summary>
    /// Maximal CID of code contained in this CMap file.
    /// </summary>
    public int MaximalCID { get; set; }
}

/// <summary>
/// JPEG2000 image.
/// </summary>
public class JPEG2000 : External
{
    /// <summary>
    /// Number of colour channels.
    /// </summary>
    public int NrColorChannels { get; set; }

    /// <summary>
    /// Number of colour space specifications.
    /// </summary>
    public int NrColorSpaceSpecs { get; set; }

    /// <summary>
    /// Number of colour space specs with APPROX equal to 0x01.
    /// </summary>
    public int NrColorSpacesWithApproxField { get; set; }

    /// <summary>
    /// METH value in the colr box where APPROX is 0x01.
    /// </summary>
    public int ColrMethod { get; set; }

    /// <summary>
    /// EnumCS value in the colr box where APPROX is 0x01.
    /// </summary>
    public int ColrEnumCS { get; set; }

    /// <summary>
    /// Image bit depth.
    /// </summary>
    public int BitDepth { get; set; }

    /// <summary>
    /// True if bpcc box is present.
    /// </summary>
    public bool BpccBoxPresent { get; set; }

    /// <summary>
    /// True if Image XObject has a /ColorSpace entry.
    /// </summary>
    public bool HasColorSpace { get; set; }
}

/// <summary>
/// Embedded file.
/// </summary>
public class EmbeddedFile : External
{
    /// <summary>
    /// MIME type of the embedded file (/Subtype).
    /// </summary>
    public string? Subtype { get; set; }

    /// <summary>
    /// True if this file is valid PDF/A part 1 or 2.
    /// </summary>
    public bool IsValidPDFA12 { get; set; }

    /// <summary>
    /// True if this file is valid PDF/A part 1, 2, or 4.
    /// </summary>
    public bool IsValidPDFA124 { get; set; }
}

/// <summary>
/// PKCS#7 binary data object representing the PDF signature.
/// </summary>
public class PKCSDataObject : External
{
    /// <summary>
    /// Number of SignerInfo structures.
    /// </summary>
    public int SignerInfoCount { get; set; }

    /// <summary>
    /// True if X.509 signing certificate is present.
    /// </summary>
    public bool SigningCertificatePresent { get; set; }
}
