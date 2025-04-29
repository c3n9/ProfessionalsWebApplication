using ProfessionalsWebApplication.Enums;

namespace ProfessionalsWebApplication.Models;

public class Answer
{
    public string Question { get; set; }
    public AnswerType Type { get; set; }
    public string Value { get; set; } 
    public FileAnswer File { get; set; } 
}