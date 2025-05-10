using System.Text.Json.Serialization;

namespace ProfessionalsWebApplication.Models;

public class AesKeyInfo
{
    public string Key { get; set; }
    public string Iv { get; set; }
}