using System.ComponentModel.DataAnnotations;

namespace ProfessionalsWebApplication.Models.DTO;

public class CompetenceDto
{
    [Required]
    public string Name { get; set; }
}