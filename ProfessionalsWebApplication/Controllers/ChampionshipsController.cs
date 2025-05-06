using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfessionalsWebApplication.Models;

namespace ProfessionalsWebApplication.Controllers;

[Route("api/Championships")]
[ApiController]
public class ChampionshipsController : Controller
{
    private readonly ProfessionalsDbContext _context;

    public ChampionshipsController(ProfessionalsDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetChampionships()
    {
        var championships = await _context.Championships.ToListAsync();
        return Ok(championships);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetChampionship(int id)
    {
        var championship = await _context.Championships.FindAsync(id);
        if (championship == null) return NotFound("Такой чемпионат не найден.");
        return Ok(championship);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateChampionship(int id, [FromBody] Championship championship)
    {
        if (id != championship.Id)
            return BadRequest("ID в URL и теле запроса не совпадают.");

        var existingChampionship = await _context.Championships.FindAsync(id);
        if (existingChampionship == null)
            return NotFound("Чемпионат не найден.");

        _context.Entry(existingChampionship).CurrentValues.SetValues(championship);
        _context.Entry(existingChampionship).Property(x => x.Id).IsModified = false;

        try
        {
            await _context.SaveChangesAsync();
            return Ok(existingChampionship);
        }
        catch (DbUpdateConcurrencyException)
        {
            var entry = _context.Entry(existingChampionship);
            await entry.ReloadAsync();
            if (entry.State == EntityState.Detached)
                return NotFound("Чемпионат был удалён.");
            else
                return Conflict("Конфликт версий. Данные были изменены другим пользователем.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteChampionship(int id)
    {
        var championship = await _context.Championships.FindAsync(id);
        if (championship == null)
        {
            return NotFound();
        }
        _context.Championships.Remove(championship);
        await _context.SaveChangesAsync();
        return Ok();
    }
}