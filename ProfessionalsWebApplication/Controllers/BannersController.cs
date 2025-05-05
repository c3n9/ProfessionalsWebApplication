using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfessionalsWebApplication.Models;

namespace ProfessionalsWebApplication.Controllers
{
	public class BannersController : Controller
	{
		private readonly ProfessionalsDbContext _context;

		public BannersController(ProfessionalsDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async Task<IActionResult> GetBanners()
		{
			var banners = await _context.Banners.ToListAsync();
			return Ok(banners);
		}

		[HttpPost]
		public async Task<IActionResult> CreateBanner([FromBody] Banner banner)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			if (_context.Banners.Any(x => x.Title == banner.Title))
				return BadRequest("С таким названием баннер уже существует.");

			_context.Banners.Add(banner);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetBanners), new { id = banner.Id }, banner);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateBanner(int id, [FromBody] Banner banner)
		{
			if (id != banner.Id)
				return BadRequest("ID в URL и теле запроса не совпадают.");

			var existingQuestion = await _context.Banners.FindAsync(id);
			if (existingQuestion == null)
				return NotFound("Баннер не найден.");

			_context.Entry(existingQuestion).CurrentValues.SetValues(banner);

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
					return NotFound("Баннер был удалён.");
				else
					return Conflict("Конфликт версий. Данные были изменены другим пользователем.");
			}
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteBanner(int id)
		{
			var banner = await _context.Banners.FindAsync(id);
			if (banner == null)
			{
				return NotFound();
			}
			_context.Banners.Remove(banner);
			await _context.SaveChangesAsync();
			return Ok();
		}
	}
}
