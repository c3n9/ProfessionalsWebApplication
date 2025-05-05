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
		[Key]
		public int Id { get; set; }
		[Required]
		public int FormId { get; set; }
		public string AnswersJson { get; set; }
		[Required]
		public DateTime Timestamp { get; set; } = DateTime.Now;
		[NotMapped]
		public List<Answer> Answers
		{
			get
			{
				var root = JsonNode.Parse(CryptoService.Decrypt(AnswersJson));
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
						Value = pair.Value.GetString()
					});
				}
			}

			return result;
		}
	}
}
