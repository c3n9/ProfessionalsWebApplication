using System.Security.Cryptography;
using System.Text;

namespace ProfessionalsWebApplication.Services
{
    public static class HashGenerator
    {
        private const string SecretKey = "your-secret-key"; // Лучше вынести в конфиг

        public static string GenerateHash(int formId)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SecretKey));
            byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(formId.ToString()));
            return Convert.ToBase64String(hashBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('='); // Делаем URL-friendly
        }

        public static bool ValidateHash(int formId, string hash)
        {
            return GenerateHash(formId) == hash;
        }
    }
}
