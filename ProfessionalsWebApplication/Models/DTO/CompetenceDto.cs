using System.ComponentModel.DataAnnotations;

namespace ProfessionalsWebApplication.Models.DTO;

public class CompetenceDto
{
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
    public List<string> Tasks {get; set;} // Задачи
    
    public IFormFile? ImageFile { get; set; }

    public int? TypeId { get; set; }

}