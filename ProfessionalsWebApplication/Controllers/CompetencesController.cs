using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfessionalsWebApplication.Models;
using ProfessionalsWebApplication.Models.DTO;

namespace ProfessionalsWebApplication.Controllers
{
    [Route("api/Competences")]
    [ApiController]
    public class CompetencesController : Controller
    {
        private readonly ProfessionalsDbContext _context;

        public CompetencesController(ProfessionalsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCompetences()
        {
            var competences = await _context.Competences.ToListAsync();
            return Ok(competences);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompetence(int id)
        {
            var competence = await _context.Competences.FindAsync(id);
            if (competence == null) return NotFound("Такая компетенция не найдена.");
            return Ok(competence);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCompetence([FromForm] CompetenceDto competenceDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var competence = new Competence
            {
                Name = competenceDto.Name
            };

            _context.Competences.Add(competence);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCompetence), new { id = competence.Id }, competence);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompetence(int id, [FromForm] CompetenceDto competenceDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingCompetence = await _context.Competences.FindAsync(id);
            if (existingCompetence == null)
                return NotFound("Компетенция не найдена.");

            existingCompetence.Name = competenceDto.Name;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(existingCompetence);
            }
            catch (DbUpdateConcurrencyException)
            {
                var entry = _context.Entry(existingCompetence);
                await entry.ReloadAsync();
                if (entry.State == EntityState.Detached)
                    return NotFound("Компетенция была удалена.");
                else
                    return Conflict("Конфликт версий. Данные были изменены другим пользователем.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompetence(int id)
        {
            var competence = await _context.Competences.FindAsync(id);
            if (competence == null)
            {
                return NotFound();
            }
            _context.Competences.Remove(competence);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}