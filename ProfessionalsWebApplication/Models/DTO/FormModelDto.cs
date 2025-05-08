using System.ComponentModel.DataAnnotations;

namespace ProfessionalsWebApplication.Models.DTO;

public class FormModelDto
{
    [Required(ErrorMessage = "Название формы обязательно")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Видимость формы обязательна")]
    public bool IsVisible { get; set; } = true;

    [Required(ErrorMessage = "Дата начала обязательна")]
    public DateTime DateStart { get; set; }

    [Required(ErrorMessage = "Дата окончания обязательна")]
    public DateTime DateEnd { get; set; }
}