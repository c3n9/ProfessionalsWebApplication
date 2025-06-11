using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ProfessionalsWebApplication.Models;

public class Competitor
{
    [Key] public int Id { get; set; }
    public string FullName { get; set; }
    public string Group { get; set; }
    [ForeignKey(nameof(Championship))] public int ChampionshipId { get; set; }
    [JsonIgnore] public Championship Championship { get; set; }
    [ForeignKey(nameof(Competence))] public int CompetenceId { get; set; }
    [JsonIgnore] public Competence Competence { get; set; }
    public string Place { get; set; }
    public string ImageUrl { get; set; }
}