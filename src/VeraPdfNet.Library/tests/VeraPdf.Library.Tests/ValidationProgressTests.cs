using VeraPdf.Library.Core.Utils;

namespace VeraPdf.Library.Tests;

public class ValidationProgressTests
{
    [Fact]
    public void TracksAndResetsValidationStatistics()
    {
        var progress = new ValidationProgress(showProgress: false);
        progress.UpdateVariables();
        progress.IncrementNumberOfChecks();
        progress.IncrementNumberOfChecks();
        progress.UpdateNumberOfFailedChecks(1);
        progress.IncrementNumberOfProcessedObjects();
        progress.UpdateNumberOfObjectsToBeProcessed(4);

        Assert.Equal("Progress: 2 checks, 1 failed, 1 processed objects, 4 in stack.",
            progress.GetCurrentValidationJobProgressWithCommas());
        Assert.Equal(2, progress.GetCurrentValidationJobStats()["checks"]);
        Assert.Equal(1, progress.GetCurrentValidationJobStats()["failedChecks"]);
        Assert.Equal(1, progress.GetCurrentValidationJobStats()["processedObjects"]);
        Assert.Equal(4, progress.GetCurrentValidationJobStats()["objectsToBeProcessed"]);

        progress.UpdateVariables();

        Assert.Equal("Progress: 0 checks, 0 failed, 0 processed objects, 1 in stack.",
            progress.GetCurrentValidationJobProgressWithCommas());
    }

    [Fact]
    public void WritesProgressToConsoleErrorWhenEnabled()
    {
        var previousError = Console.Error;
        var previousOutput = Console.Out;
        using var output = new StringWriter();
        using var standardOutput = new StringWriter();
        try
        {
            Console.SetError(output);
            Console.SetOut(standardOutput);
            var progress = new ValidationProgress(showProgress: true);
            progress.UpdateVariables();
            progress.IncrementNumberOfChecks();
            progress.ShowProgressAfterValidation();
        }
        finally
        {
            Console.SetError(previousError);
            Console.SetOut(previousOutput);
        }

        Assert.Contains("Progress: 1 checks / 0 failed / 0 processed objects / 1 in stack.", output.ToString());
        Assert.EndsWith(Environment.NewLine, output.ToString());
    }
}
