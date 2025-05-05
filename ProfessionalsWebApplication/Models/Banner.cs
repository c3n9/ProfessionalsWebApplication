using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProfessionalsWebApplication.Models
{
	public class Banner
	{
		[Key]
		public int Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string ImagePath { get; set; }
		public string LinkToButton { get; set; }
	}
}
