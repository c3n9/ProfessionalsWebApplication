using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProfessionalsWebApplication.Models
{
    public class FormModel
    {
        [Key]
        public int Id { get; set; }
		public string Hash { get; set; }
        public string Name { get; set; } = string.Empty;
        
        public bool IsVisible { get; set; } = true;
        public List<QuestionModel> Questions { get; set; } = new();
    }
}
