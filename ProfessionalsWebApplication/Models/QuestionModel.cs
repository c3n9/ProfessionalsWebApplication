using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ProfessionalsWebApplication.Models
{
    public class QuestionModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
		public string Type { get; set; } = string.Empty;
        [Required]
		[MaxLength(150)]
		public string Text { get; set; } = string.Empty;
        [Required]
		public bool IsRequired { get; set; } = false;
        [Required]
		public bool IsDropDown {get;set;} = false;
		public string Note { get; set; } = string.Empty;
        [Required]
        [MaxLength(300)]
		public string DisplayText { get; set; } = string.Empty;
		public List<string>? Options { get; set; }

		[ForeignKey(nameof(FormModel))]  
		public int ThemeId { get; set; }

		[JsonIgnore]
		public FormModel? FormModel { get; set; }
	}
}
