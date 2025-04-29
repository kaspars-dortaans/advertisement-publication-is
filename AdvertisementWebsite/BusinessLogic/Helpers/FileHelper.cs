using System.Security.Cryptography;

namespace BusinessLogic.Helpers;

public class FileHelper
{
    public static async Task<string> GetFileHash(Stream fileStream)
    {
        using var sha256 = SHA256.Create();
        fileStream.Seek(0, SeekOrigin.Begin);
        var fileHash = await sha256.ComputeHashAsync(fileStream);
        return BitConverter.ToString(fileHash).Replace("-", "");
    }
}
