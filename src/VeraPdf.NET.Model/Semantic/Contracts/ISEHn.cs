namespace VeraPdf.NET.Model.Contracts.Semantic;

public interface ISEHn
{
    bool HasCorrectNestingLevel { get; }

    int NestingLevel { get; }
}
