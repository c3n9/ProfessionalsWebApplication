using System.Security.Cryptography;
using System.Text;

namespace ProfessionalsWebApplication.Services;

public class CryptoService
{
    public static string Decrypt(string encryptedData)
    {
        using (var rsa = new RSACryptoServiceProvider())
        {
            string privateKey = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Key", "privateKey.xml"));
            var byteEncryptedData = Convert.FromBase64String(encryptedData);
            rsa.FromXmlString(privateKey);
            byte[] decryptedData = rsa.Decrypt(byteEncryptedData, false);
            return Encoding.UTF8.GetString(decryptedData);
        }
    }
}