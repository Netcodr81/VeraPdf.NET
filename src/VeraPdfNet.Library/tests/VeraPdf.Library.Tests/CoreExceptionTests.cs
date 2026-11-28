using VeraPdf.Library.Core;

namespace VeraPdf.Library.Tests;

public class CoreExceptionTests
{
    [Fact]
    public void VeraPdfExceptionPreservesMessageAndInnerException()
    {
        var cause = new InvalidDataException("cause");

        var exception = new VeraPdfException("failure", cause);

        Assert.Equal("failure", exception.Message);
        Assert.Same(cause, exception.InnerException);
    }

    [Fact]
    public void EncryptedPdfExceptionIsAVeraPdfExceptionAndPreservesCause()
    {
        var cause = new IOException("encrypted");

        var exception = new EncryptedPdfException("cannot read PDF", cause);

        Assert.IsAssignableFrom<VeraPdfException>(exception);
        Assert.Equal("cannot read PDF", exception.Message);
        Assert.Same(cause, exception.InnerException);
    }

    [Fact]
    public void CoreExceptionTypesPreserveMessageAndCause()
    {
        var cause = new InvalidDataException("cause");
        VeraPdfException[] exceptions =
        [
            new ProfileException("profile", cause),
            new ValidationException("validation", cause),
            new ModelParsingException("model", cause),
            new FeatureParsingException("feature", cause)
        ];
        string[] messages = ["profile", "validation", "model", "feature"];

        for (var index = 0; index < exceptions.Length; index++)
        {
            Assert.Equal(messages[index], exceptions[index].Message);
            Assert.Same(cause, exceptions[index].InnerException);
        }
    }
}
