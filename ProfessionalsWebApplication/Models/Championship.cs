using System.ComponentModel.DataAnnotations;

namespace ProfessionalsWebApplication.Models;

public partial class Championship
{
    [Key]
    public int Id { get; set; } 
    [Required]
    public string Name { get; set; }
}