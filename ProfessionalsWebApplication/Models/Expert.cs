using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProfessionalsWebApplication.Models
{
	public class Expert
	{
		[Key] public int Id { get; set; }
		public string FullName { get; set; }
		public string Post { get; set; }
		[ForeignKey(nameof(Competence))] public int CompetenceId { get; set; }
		[JsonIgnore] public Competence Competence { get; set; }
		public string ImageUrl { get; set; }
	}
}
