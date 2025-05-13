using System.Security.Cryptography;
using System.Text;

namespace ProfessionalsWebApplication.Services;

public class CryptoService
{
    public static string DecryptRSA(string encryptedData)
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
    
    public static string DecryptAes(byte[] encryptedData, byte[] key, byte[] iv)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (ICryptoTransform decryptor = aes.CreateDecryptor())
            using (MemoryStream ms = new MemoryStream(encryptedData))
            using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (StreamReader sr = new StreamReader(cs, Encoding.UTF8))
            {
                return sr.ReadToEnd(); // Возвращает расшифрованную строку
            }
        }
    }
}