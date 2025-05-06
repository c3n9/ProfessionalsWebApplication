using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfessionalsWebApplication.Models;

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
            var competence = await _context.Competences.ToListAsync();
            return Ok(competence);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompetence(int id)
        {
            var competence = await _context.Competences.FindAsync(id);
            if (competence == null) return NotFound("Такая компетенция не найден.");
            return Ok(competence);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompetence(int id, [FromBody] Competence competence)
        {
            if (id != competence.Id)
                return BadRequest("ID в URL и теле запроса не совпадают.");

            var existingCompetence = await _context.Competences.FindAsync(id);
            if (existingCompetence == null)
                return NotFound("Компетенция не найден.");

            _context.Entry(existingCompetence).CurrentValues.SetValues(competence);
            _context.Entry(existingCompetence).Property(x => x.Id).IsModified = false;

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
                    return NotFound("Компетенция был удалён.");
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
