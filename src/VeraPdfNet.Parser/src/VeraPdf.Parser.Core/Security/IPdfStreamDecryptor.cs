using VeraPdf.Parser.Cos;

namespace VeraPdf.Parser.Security;

public interface IPdfStreamDecryptor
{
    byte[] Decrypt(byte[] data, CosKey objectKey);
    byte[] DecryptString(byte[] data, CosKey objectKey);
}
