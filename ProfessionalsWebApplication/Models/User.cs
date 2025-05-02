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
		public string FormId { get; set; }
		public string AnswersJson { get; set; }
		[Required]
		public DateTime Timestamp { get; set; } = DateTime.Now;
		[NotMapped]
		public List<Answer> Answers
		{
			get => string.IsNullOrEmpty(AnswersJson) 
				? new List<Answer>() 
				: JsonSerializer.Deserialize<List<Answer>>(AnswersJson);

			set => AnswersJson = JsonSerializer.Serialize(value);
		}
	}
}
