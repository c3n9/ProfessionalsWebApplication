using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProfessionalsWebApplication.Models.DTO
{
	public class ExpertDto
	{
		[Required]
		public string FullName { get; set; }
		[Required]
		public string Post { get; set; }
		[Required]
		public int CompetenceId { get; set; }
		public IFormFile ImageFile { get; set; }

	}
}
