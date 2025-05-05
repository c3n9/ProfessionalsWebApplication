using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProfessionalsWebApplication.Models
{
	public class Banner
	{
		[Key]
		public int Id { get; set; }
		[JsonPropertyName("title")]
		public string Title { get; set; }
		[JsonPropertyName("text")]
		public string Description { get; set; }
		[JsonPropertyName("img")]
		public string ImagePath { get; set; }
		[JsonPropertyName("buttonLink")]
		public string LinkToButton { get; set; }
	}
}
