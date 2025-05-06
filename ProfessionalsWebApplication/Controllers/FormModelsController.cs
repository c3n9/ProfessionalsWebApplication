using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfessionalsWebApplication.Models;
using ProfessionalsWebApplication.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
		public async Task<IActionResult> CreateForm([FromBody] FormModel formModel)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			_context.Forms.Add(formModel);
			await _context.SaveChangesAsync();

			formModel.Hash = HashGenerator.GenerateHash(formModel.Id);
		    _context.Forms.Update(formModel);
			await _context.SaveChangesAsync();


			//var location = Url.Link(nameof(GetForm), new { id = formModel.Id });

			//return Created(location, formModel);

			return CreatedAtAction(nameof(GetForm), new { id = formModel.Id }, formModel);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateForm(int id, [FromBody] FormModel formModel)
		{
			if (id != formModel.Id)
				return BadRequest("ID в URL и теле запроса не совпадают.");

			var existingForm = await _context.Forms.FindAsync(id);
			if (existingForm == null)
				return NotFound("Форма не найдена.");

			_context.Entry(existingForm).CurrentValues.SetValues(formModel);
			_context.Entry(existingForm).Property(x => x.Id).IsModified = false;
			_context.Entry(existingForm).Property(x => x.Hash).IsModified = false;

			try
			{
				await _context.SaveChangesAsync();
				return Ok(existingForm);
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
