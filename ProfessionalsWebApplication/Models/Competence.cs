using System.ComponentModel.DataAnnotations;

namespace ProfessionalsWebApplication.Models;

public class Competence
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }    
}