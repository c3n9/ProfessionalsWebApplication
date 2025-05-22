using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfessionalsWebApplication.Models;
using ProfessionalsWebApplication.Models.DTO;

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

    [HttpPost]
    public async Task<IActionResult> CreateChampionship([FromForm] ChampionshipDto championshipDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var championship = new Championship
        {
            Name = championshipDto.Name,
            Year = championshipDto.Year,
            TypeId = championshipDto.TypeId,
        };

        _context.Championships.Add(championship);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetChampionship), new { id = championship.Id }, championship);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateChampionship(int id, [FromForm] ChampionshipDto championshipDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingChampionship = await _context.Championships.FindAsync(id);
        if (existingChampionship == null)
            return NotFound("Чемпионат не найден.");

        existingChampionship.Name = championshipDto.Name;
        existingChampionship.Year = championshipDto.Year;
        existingChampionship.TypeId = championshipDto.TypeId;

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