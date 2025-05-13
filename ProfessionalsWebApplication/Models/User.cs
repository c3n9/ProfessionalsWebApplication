using ProfessionalsWebApplication.Enums;
using ProfessionalsWebApplication.Models.DTO;
using ProfessionalsWebApplication.Services;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ProfessionalsWebApplication.Models
{
    public class User
    {
        [Key] public int Id { get; set; }
        [Required] public int FormId { get; set; }
        public string Key { get; set; }
        public string Iv { get; set; }
        public string AnswersJson { get; set; }
        [Required] public DateTime Timestamp { get; set; } = DateTime.Now;

        [NotMapped]
        public List<Answer> Answers
        {
            get
            {
                var root = JsonNode.Parse(AnswersJson);
                string answersNode = root?["Answers"].ToString();
                var answers = MigrateOldJson(answersNode);
                return answers;
            }
        }

        private List<Answer> MigrateOldJson(string oldJson)
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(oldJson);
            var result = new List<Answer>();

            foreach (var pair in dict)
            {
                if (pair.Value.ValueKind == JsonValueKind.Object)
                {
                    var file = pair.Value.Deserialize<FileAnswer>();
                    file.FileContent = CryptoService.DecryptAes(Convert.FromBase64String(file.FileContent),
                        Convert.FromBase64String(Key), Convert.FromBase64String(Iv));
                    result.Add(new Answer
                    {
                        Question = pair.Key,
                        Type = AnswerType.File,
                        File = file
                    });
                }
                else
                {
                    result.Add(new Answer
                    {
                        Question = pair.Key,
                        Type = AnswerType.Text,
                        Value = CryptoService.DecryptAes(Convert.FromBase64String(pair.Value.GetString()), Convert.FromBase64String(Key), Convert.FromBase64String(Iv))
                    });
                }
            }

            return result;
        }
    }
}