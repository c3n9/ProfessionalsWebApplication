using System.ComponentModel.DataAnnotations;

namespace ProfessionalsWebApplication.Models.DTO
{
	public class LinkDto
	{
		[Required]
		[Url(ErrorMessage = "Неверный формат ссылки")]
		public string Url { get; set; }
	}
}
