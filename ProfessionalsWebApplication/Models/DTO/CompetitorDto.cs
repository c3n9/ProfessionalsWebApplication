using Microsoft.Build.Framework;

namespace ProfessionalsWebApplication.Models.DTO;

public class CompetitorDto
{
    [Required]
    public string FullName { get; set; }
    [Required]
    public string Group { get; set; }
    [Required]
    public int ChampionshipId { get; set; }
    [Required]
    public int CompetenceId { get; set; }
    [Required]
    public string Place  { get; set; }
    public IFormFile? ImageFile { get; set; }
}