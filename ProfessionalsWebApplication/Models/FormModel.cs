using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProfessionalsWebApplication.Models
{
    public class FormModel
    {
        [Key]
        public int Id { get; set; }
        public string Hash { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public bool IsVisible { get; set; } = true;
        [Required]
        public DateTime DateStart { get; set; }
        [Required]
        public DateTime DateEnd { get; set; }
        public List<QuestionModel> Questions { get; set; } = new();
        public List<User> Users { get; set; } = new();
    }
}
