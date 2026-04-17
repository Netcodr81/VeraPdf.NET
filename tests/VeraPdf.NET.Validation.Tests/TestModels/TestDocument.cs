namespace VeraPdf.NET.Validation.Tests.TestModels;

/// <summary>
/// Root test document.
/// Mimics PDF structure.
/// </summary>
public class TestDocument
{
    public List<TestPage> Pages { get; set; } = new();
}