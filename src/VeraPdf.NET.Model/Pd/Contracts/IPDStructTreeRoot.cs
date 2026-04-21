namespace VeraPdf.NET.Model.Pd.Contracts;

public interface IPDStructTreeRoot
{
    string? FirstChildStandardTypeNamespaceURL { get; }

    int RoleMapNamesCount { get; }
}
