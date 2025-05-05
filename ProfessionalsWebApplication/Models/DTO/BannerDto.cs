using System.ComponentModel.DataAnnotations;

namespace ProfessionalsWebApplication.Models.DTO
{
	public class BannerDto
	{
		[Required]
		public string Title { get; set; }

		public string Description { get; set; }

		[Required]
		public IFormFile ImageFile { get; set; } 

		public string LinkToButton { get; set; }
	}
}
