using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfessionalsWebApplication.Models;

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
		public async Task<IActionResult> CreateQuestion([FromBody] QuestionModel questionModel)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			if(_context.Questions.Any(x => x.DisplayText == questionModel.DisplayText || x.Text == questionModel.Text))
				return BadRequest("С таким названием или кратким названием уже существует.");

			_context.Questions.Add(questionModel);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetQuestion), new { id = questionModel.Id }, questionModel);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateQuestion(int id, [FromBody] QuestionModel questionModel)
		{
			if (id != questionModel.Id)
				return BadRequest("ID в URL и теле запроса не совпадают.");

			var existingQuestion = await _context.Questions.FindAsync(id);
			if (existingQuestion == null)
				return NotFound("Вопрос не найден.");

			_context.Entry(existingQuestion).CurrentValues.SetValues(questionModel);
			_context.Entry(existingQuestion).Property(x => x.Id).IsModified = false;

			try
			{
				await _context.SaveChangesAsync();
				return Ok(existingQuestion);
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
