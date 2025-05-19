using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfessionalsWebApplication.Models;
using ProfessionalsWebApplication.Models.DTO;

namespace ProfessionalsWebApplication.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class LinksController : Controller
	{
		private readonly ProfessionalsDbContext _context;
		public LinksController(ProfessionalsDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async Task<IActionResult> GetLinks()
		{
			var links = await _context.Links.ToListAsync();

			var result = links.Select(l => new
			{
				l.Id,
				l.Name,
				l.Url,
			});
			return Ok(result);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetLink(int id)
		{
			var link = await _context.Links.FindAsync(id);
			if (link == null) return NotFound("Такая ссылка не найдена");

			var result = new
			{
				link.Id,
				link.Name,
				link.Url,
			};
			return Ok(result);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateLink(int id, [FromForm] LinkDto linkDto)
		{
			var existingLink = await _context.Links.FindAsync(id);
			if(existingLink == null)
				return NotFound("Ссылка не найдена");

			existingLink.Url = linkDto.Url;

			try
			{
				await _context.SaveChangesAsync();
				return Ok(new
				{
					existingLink.Id,
					existingLink.Name,
					existingLink.Url
				});
			}
			catch (DbUpdateConcurrencyException)
			{
				var entry = _context.Entry(existingLink);
				await entry.ReloadAsync();
				if (entry.State == EntityState.Detached)
					return NotFound("Ссылка была удалена.");
				else
					return Conflict("Конфликт версий. Данные были изменены другим пользователем.");
			}
		}
	}
}
