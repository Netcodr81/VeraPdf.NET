using VeraPdf.Library.Core;

namespace VeraPdf.Library.Core.Utils;

public interface IFileOutputMapper
{
    string Prefix { get; }
    string Suffix { get; }
    FileInfo MapFile(FileInfo original);
}
