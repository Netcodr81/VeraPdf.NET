namespace VeraPdf.NET.Model.Contracts.StructureAnalysis;

public interface ISAPDFDocument
{
    int PagesCount { get; }

    int RepeatedCharactersCount { get; }

    int ListsCount { get; }

    bool ContainsStructTreeRoot { get; }
}
