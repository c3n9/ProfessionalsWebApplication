using System.ComponentModel.DataAnnotations;

namespace ProfessionalsWebApplication.Models;

public class Competence
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } 
    
    [Required]
    public string Description { get; set; }
    
    [Required]
    public string Direction {get; set;} // Направление
    
    [Required]
    public string Category {get; set;} // Категория
    
    [Required] 
    public string Soft { get; set; } // Программное обеспечение/среда разработки
    
    [Required]
    public string Tasks {get; set;} // Задачи
    
    [Required]
    public string ImageUrl { get; set; } 
}