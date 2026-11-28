namespace VeraPdf.Library.Core.Utils;

public sealed class ValidationProgress
{
    private const int UpperLimitPowerOfTen = 6;
    private const int UpperPowerOfTenStep = 5;
    private static readonly double UpperLimit = Math.Pow(10, UpperLimitPowerOfTen);
    private static readonly double UpperLimitStep = Math.Pow(10, UpperPowerOfTenStep);

    private readonly bool _showProgress;
    private double _currentStep = 1;
    private int _currentPowerOfTenStep;
    private int _numberOfChecks;
    private int _numberOfFailedChecks;
    private int _numberOfProcessedObjects;
    private int _numberOfObjectsToBeProcessed;
    private int _lengthOfPreviousProgressString = 1;

    public ValidationProgress(bool showProgress)
    {
        _showProgress = showProgress;
    }

    public void UpdateVariables()
    {
        if (_showProgress)
        {
            Console.Out.WriteLine();
        }

        _numberOfChecks = 0;
        _numberOfFailedChecks = 0;
        _numberOfProcessedObjects = 0;
        _numberOfObjectsToBeProcessed = 1;
    }

    public void IncrementNumberOfChecks()
    {
        ++_numberOfChecks;
        if (_showProgress && CheckCurrentNumberOfChecks())
        {
            ShowProgressDuringValidation();
            CheckAndChangeCurrentStep();
        }
    }

    public void ShowProgressAfterValidation()
    {
        if (_showProgress)
        {
            Console.Error.WriteLine(GetCurrentValidationJobProgress().PadRight(_lengthOfPreviousProgressString));
        }
    }

    public void UpdateNumberOfFailedChecks(int numberOfFailedChecks) =>
        _numberOfFailedChecks = numberOfFailedChecks;

    public void IncrementNumberOfProcessedObjects() => ++_numberOfProcessedObjects;

    public void UpdateNumberOfObjectsToBeProcessed(int numberOfObjectsToBeProcessed) =>
        _numberOfObjectsToBeProcessed = numberOfObjectsToBeProcessed;

    public string GetCurrentValidationJobProgressWithCommas() =>
        $"Progress: {_numberOfChecks} checks, {_numberOfFailedChecks} failed, " +
        $"{_numberOfProcessedObjects} processed objects, {_numberOfObjectsToBeProcessed} in stack.";

    public Dictionary<string, object> GetCurrentValidationJobStats() => new()
    {
        ["checks"] = _numberOfChecks,
        ["failedChecks"] = _numberOfFailedChecks,
        ["processedObjects"] = _numberOfProcessedObjects,
        ["objectsToBeProcessed"] = _numberOfObjectsToBeProcessed
    };

    private string GetCurrentValidationJobProgress() =>
        $"Progress: {_numberOfChecks} checks / {_numberOfFailedChecks} failed / " +
        $"{_numberOfProcessedObjects} processed objects / {_numberOfObjectsToBeProcessed} in stack.";

    private bool CheckCurrentNumberOfChecks() => _numberOfChecks % _currentStep == 0;

    private void CheckAndChangeCurrentStep()
    {
        if (_numberOfChecks > UpperLimit)
        {
            return;
        }

        if (_numberOfChecks == Math.Pow(10, _currentPowerOfTenStep + 1))
        {
            ++_currentPowerOfTenStep;
            _currentStep = Math.Pow(10, _currentPowerOfTenStep);
        }

        if (_numberOfChecks == UpperLimit)
        {
            _currentStep = UpperLimitStep;
        }
    }

    private void ShowProgressDuringValidation()
    {
        var currentProgress = GetCurrentValidationJobProgress();
        Console.Error.Write(currentProgress.PadRight(_lengthOfPreviousProgressString) + "\r");
        _lengthOfPreviousProgressString = currentProgress.Length;
    }
}
