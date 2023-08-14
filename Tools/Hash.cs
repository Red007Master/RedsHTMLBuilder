using System.Security.Cryptography;
using System.Text;

public class Hash
{
    public static string GetStringSHA256(string value)
    {
        using var hash = SHA256.Create();
        var byteArray = hash.ComputeHash(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(byteArray);
    }

    public static string GetFileSHA256(string filePath)
    {
        using (SHA256 SHA256 = SHA256.Create())
        {
            using (FileStream fileStream = File.OpenRead(filePath))
                return Convert.ToHexString(SHA256.ComputeHash(fileStream));
        }
    }

}
