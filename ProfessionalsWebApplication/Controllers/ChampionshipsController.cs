using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfessionalsWebApplication.Models;

namespace ProfessionalsWebApplication.Controllers;

[Route("api/FormModels")]
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
    public async Task<IActionResult> GetChampionships(int id)
    {
        var championship = await _context.Championships.FindAsync(id);
        if (championship == null) return NotFound("Такой чемпионат не найден.");
        return Ok(championship);
    }
}