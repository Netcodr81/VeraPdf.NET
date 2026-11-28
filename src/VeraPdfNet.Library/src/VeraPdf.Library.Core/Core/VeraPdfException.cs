namespace VeraPdf.Library.Core;

/// <summary>Base exception for errors raised by the veraPDF library.</summary>
public class VeraPdfException : Exception
{
    public VeraPdfException()
    {
    }

    public VeraPdfException(string? message)
        : base(message)
    {
    }

    public VeraPdfException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>Indicates that a PDF is encrypted and cannot be processed.</summary>
public class EncryptedPdfException : VeraPdfException
{
    public EncryptedPdfException()
    {
    }

    public EncryptedPdfException(string? message)
        : base(message)
    {
    }

    public EncryptedPdfException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>Indicates that a validation profile could not be parsed.</summary>
public class ProfileException : VeraPdfException
{
    public ProfileException()
    {
    }

    public ProfileException(string? message)
        : base(message)
    {
    }

    public ProfileException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>Indicates an error while performing validation.</summary>
public class ValidationException : VeraPdfException
{
    public ValidationException()
    {
    }

    public ValidationException(string? message)
        : base(message)
    {
    }

    public ValidationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>Indicates an error while parsing a PDF model.</summary>
public class ModelParsingException : VeraPdfException
{
    public ModelParsingException()
    {
    }

    public ModelParsingException(string? message)
        : base(message)
    {
    }

    public ModelParsingException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>Indicates an error while parsing PDF features.</summary>
public class FeatureParsingException : VeraPdfException
{
    public FeatureParsingException()
    {
    }

    public FeatureParsingException(string? message)
        : base(message)
    {
    }

    public FeatureParsingException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
