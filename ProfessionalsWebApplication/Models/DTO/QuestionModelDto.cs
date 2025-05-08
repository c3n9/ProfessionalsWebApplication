using System.ComponentModel.DataAnnotations;

namespace ProfessionalsWebApplication.Models.DTO;

public class QuestionModelDto
{
    [Required(ErrorMessage = "Тип вопроса обязателен")]
    public string Type { get; set; } = string.Empty;

    [Required(ErrorMessage = "Текст вопроса обязателен")]
    public string Text { get; set; } = string.Empty;

    [Required(ErrorMessage = "Поле IsRequired обязательно")]
    public bool IsRequired { get; set; } = false;

    [Required(ErrorMessage = "Поле IsDropDown обязательно")]
    public bool IsDropDown { get; set; } = false;

    public string? Note { get; set; }

    [Required(ErrorMessage = "Отображаемый текст обязателен")]
    public string DisplayText { get; set; } = string.Empty;

    public List<string>? Options { get; set; }

    [Required(ErrorMessage = "ID темы обязательно")]
    public int ThemeId { get; set; }
}