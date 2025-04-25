using System.ComponentModel.DataAnnotations;

namespace ProfessionalsWebApplication.Models
{
    public class QuestionModel
    {
        [Key]
        public int Id { get; set; }
		public string Type { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool IsRequired { get; set; } = false;
        public bool IsDropDown {get;set;} = false;
        public string Note { get; set; } = string.Empty;
		public string DisplayText { get; set; } = string.Empty;
        public List<string>? Options { get; set; }
    }
}
