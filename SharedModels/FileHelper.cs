using System.Security.Cryptography;
using System.Text;

public static class FileHelper
{
    public static string GenerateHash(byte[] fileBytes)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(fileBytes);

        return Convert.ToBase64String(hashBytes);
    }
}