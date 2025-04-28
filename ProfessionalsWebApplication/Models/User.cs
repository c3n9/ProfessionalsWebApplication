using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace ProfessionalsWebApplication.Models
{
	public class User
	{
		[Key]
		public int Id { get; set; }
		[Required]
		public string FormName { get; set; }

		public string AnswersJson { get; set; }
		[Required]

		public DateTime Timestamp { get; set; }

		[NotMapped]
		public Dictionary<string, object> Answers
		{
			get => AnswersJson == null ? new Dictionary<string, object>()
									   : JsonSerializer.Deserialize<Dictionary<string, object>>(AnswersJson);
			set => AnswersJson = JsonSerializer.Serialize(value);
		}
	}
}
