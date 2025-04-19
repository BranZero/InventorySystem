using System.Security.Cryptography;
using System.Text;

namespace InventorySystem.ServerScripts;
public class EncryptData
{
    public static string? HashSha384(string input)
    {
        byte[] bytes = SHA384.HashData(Encoding.UTF8.GetBytes(input));
        StringBuilder stringBuilder = new(bytes.Length);
        for (int i = 0; i < bytes.Length; i++)
        {
            stringBuilder.Append((char)bytes[i]);
        }
        return stringBuilder.ToString();
    }   
}