using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfessionalsWebApplication.Models;
using ProfessionalsWebApplication.Models.DTO;

namespace ProfessionalsWebApplication.Controllers
{
	[Route("api/QuestionModels")]
	[ApiController]
	public class QuestionModelsController : Controller
	{
		private readonly ProfessionalsDbContext _context;
		public QuestionModelsController(ProfessionalsDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async Task<IActionResult> GetQuestions()
		{
			var questions = await _context.Questions.ToListAsync();
			return Ok(questions);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetQuestion(int id)
		{
			var question = await _context.Questions.FindAsync(id);
			if (question == null) return NotFound("Такой вопрос не найдена.");
			return Ok(question);
		}

		[HttpPost]
		public async Task<IActionResult> CreateQuestion([FromForm] QuestionModelDto questionDto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			if (_context.Questions.Any(x => x.DisplayText == questionDto.DisplayText || x.Text == questionDto.Text))
				return BadRequest("Вопрос с таким названием или текстом уже существует.");

			var questionModel = new QuestionModel
			{
				Type = questionDto.Type,
				Text = questionDto.Text,
				IsRequired = questionDto.IsRequired,
				IsDropDown = questionDto.IsDropDown,
				Note = questionDto.Note,
				DisplayText = questionDto.DisplayText,
				Options = questionDto.Options,
				ThemeId = questionDto.ThemeId
			};

			_context.Questions.Add(questionModel);
			await _context.SaveChangesAsync();

			var baseUrl = $"{Request.Scheme}://{Request.Host}";
			var questionUrl = $"{baseUrl}/api/QuestionModels/{questionModel.Id}";

			return Created(questionUrl, new 
			{
				Question = questionModel,
				Url = questionUrl
			});
		}

		[HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuestion(int id, [FromForm] QuestionModelDto questionDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingQuestion = await _context.Questions.FindAsync(id);
            if (existingQuestion == null)
                return NotFound("Вопрос не найден.");

            // Проверка на дубликаты (исключая текущий вопрос)
            if (_context.Questions.Any(x => 
                (x.DisplayText == questionDto.DisplayText || x.Text == questionDto.Text) 
                && x.Id != id))
            {
                return BadRequest("Вопрос с таким названием или текстом уже существует.");
            }

            existingQuestion.Type = questionDto.Type;
            existingQuestion.Text = questionDto.Text;
            existingQuestion.IsRequired = questionDto.IsRequired;
            existingQuestion.IsDropDown = questionDto.IsDropDown;
            existingQuestion.Note = questionDto.Note;
            existingQuestion.DisplayText = questionDto.DisplayText;
            existingQuestion.Options = questionDto.Options;
            existingQuestion.ThemeId = questionDto.ThemeId;

            try
            {
                await _context.SaveChangesAsync();

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var questionUrl = $"{baseUrl}/api/QuestionModels/{existingQuestion.Id}";

                return Ok(new 
                {
                    Question = existingQuestion,
                    Url = questionUrl
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                var entry = _context.Entry(existingQuestion);
                await entry.ReloadAsync();
                if (entry.State == EntityState.Detached)
                    return NotFound("Вопрос был удалён.");
                else
                    return Conflict("Конфликт версий. Данные были изменены другим пользователем.");
            }
        }

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteQuestion(int id)
		{
			var questionModel = await _context.Questions.FindAsync(id);
			if (questionModel == null)
			{
				return NotFound();
			}
			_context.Questions.Remove(questionModel);
			await _context.SaveChangesAsync();
			return Ok();
		}
	}
}
