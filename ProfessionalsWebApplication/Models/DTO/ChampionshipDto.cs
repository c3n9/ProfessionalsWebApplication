using System.ComponentModel.DataAnnotations;

namespace ProfessionalsWebApplication.Models.DTO;

public class ChampionshipDto
{
    [Required]
    public string Name { get; set; }
    [Required]
    public int Year { get; set; }
    [Required]
    public int? TypeId { get; set; }

}