using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfessionalsWebApplication.Models;
using ProfessionalsWebApplication.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProfessionalsWebApplication.Models.DTO;

namespace ProfessionalsWebApplication.Controllers
{
	[Route("api/FormModels")]
	[ApiController]
	public class FormModelsController : Controller
	{
		private readonly ProfessionalsDbContext _context;

		public FormModelsController(ProfessionalsDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async Task<IActionResult> GetForms()
		{
			var forms = await _context.Forms.Include(f => f.Questions).ToListAsync();
			return Ok(forms);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetForm(int id)
		{
			var form = await _context.Forms.FindAsync(id);
			if (form == null) return NotFound("Такая форма не найдена.");
			return Ok(form);
		}

		[HttpPost]
		public async Task<IActionResult> CreateForm([FromForm] FormModelDto formModelDto)
		{
			if (!ModelState.IsValid) 
				return BadRequest(ModelState);

			var formModel = new FormModel
			{
				Name = formModelDto.Name,
				Hash = string.Empty,
				IsVisible = formModelDto.IsVisible,
				DateStart = formModelDto.DateStart,
				DateEnd = formModelDto.DateEnd
			};

			_context.Forms.Add(formModel);
			await _context.SaveChangesAsync();

			formModel.Hash = HashGenerator.GenerateHash(formModel.Id);
			_context.Forms.Update(formModel);
			await _context.SaveChangesAsync();

			var baseUrl = $"{Request.Scheme}://{Request.Host}";
			var formUrl = $"{baseUrl}/forms/{formModel.Hash}";

			return Created(formUrl, new 
			{
				Form = formModel,
				Url = formUrl
			});
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateForm(int id, [FromForm] FormModelDto formModelDto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var existingForm = await _context.Forms.FindAsync(id);
			if (existingForm == null)
				return NotFound("Форма не найдена.");

			existingForm.Name = formModelDto.Name;
			existingForm.IsVisible = formModelDto.IsVisible;
			existingForm.DateStart = formModelDto.DateStart;
			existingForm.DateEnd = formModelDto.DateEnd;

			try
			{
				await _context.SaveChangesAsync();

				var baseUrl = $"{Request.Scheme}://{Request.Host}";
				var formUrl = $"{baseUrl}/forms/{existingForm.Hash}";

				return Ok(new 
				{
					Form = existingForm,
					Url = formUrl
				});
			}
			catch (DbUpdateConcurrencyException)
			{
				var entry = _context.Entry(existingForm);
				await entry.ReloadAsync();

				if (entry.State == EntityState.Detached)
					return NotFound("Форма была удалена.");
				else
					return Conflict("Конфликт версий. Данные были изменены другим пользователем.");
			}
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteForm(int id)
		{
			var formModel = await _context.Forms.FindAsync(id);
			if (formModel == null)
			{
				return NotFound();
			}
			_context.Forms.Remove(formModel);
			await _context.SaveChangesAsync();
			return Ok();
		}
	}
}
