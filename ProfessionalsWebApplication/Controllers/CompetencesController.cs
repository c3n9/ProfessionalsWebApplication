using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProfessionalsWebApplication.Models;
using ProfessionalsWebApplication.Models.DTO;
using ProfessionalsWebApplication.Models.Settings;

namespace ProfessionalsWebApplication.Controllers
{
    [Route("api/Competences")]
    [ApiController]
    public class CompetencesController : Controller
    {
        private readonly ProfessionalsDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly string _competenceImagesPath;

        public CompetencesController(ProfessionalsDbContext context,
            IWebHostEnvironment env,
            IOptions<FileStorageSettings> fileStorageSettings)
        {
            _context = context;
            _env = env;
    
            var webRoot = _env.WebRootPath;
    
            if (webRoot == null)
            {
                webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                if (!Directory.Exists(webRoot))
                {
                    Directory.CreateDirectory(webRoot);
                }
                _env.WebRootPath = webRoot;
            }
    
            // Получаем путь из конфигурации
            var imagesPath = fileStorageSettings?.Value?.CompetenceImagesPath ?? "uploads/competences";
    
            // Комбинируем пути
            _competenceImagesPath = Path.Combine(webRoot, imagesPath);
    
            // Создаем целевую директорию
            Directory.CreateDirectory(_competenceImagesPath);
        }

        [HttpGet]
        public async Task<IActionResult> GetCompetences()
        {
            var competences = await _context.Competences.ToListAsync();

            var result = competences.Select(c => new
            {
                c.Id,
                c.Name,
                c.Description,
                c.Direction,
                c.Category,
                c.Soft,
                c.Tasks,
                c.TypeId,
                ImageUrl = GetImageUrl(c.ImageUrl),
            });
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompetence(int id)
        {
            var competence = await _context.Competences.FindAsync(id);
            if (competence == null) return NotFound("Такая компетенция не найдена.");
    
            var result = new
            {
                competence.Id,
                competence.Name,
                competence.Description,
                competence.Direction,
                competence.Category,
                competence.Soft,
                competence.Tasks,
                competence.TypeId,
                ImageUrl = GetImageUrl(competence.ImageUrl)
            };
    
            return Ok(result);
        }
        
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateCompetence([FromForm] CompetenceDto competenceDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_context.Competences.Any(x => x.Name == competenceDto.Name))
                return BadRequest("С таким названием компетенция уже существует.");
            
            string imagePath = null;
            if (competenceDto.ImageFile != null && competenceDto.ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(competenceDto.ImageFile.FileName);
                var filePath = Path.Combine(_competenceImagesPath, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await competenceDto.ImageFile.CopyToAsync(stream);
                }
                
                imagePath = Path.Combine("uploads/competences", fileName); // Relative path
            }

            if (competenceDto.TypeId == null)
                competenceDto.TypeId = 1;
            
            var competence = new Competence
            {
                Name = competenceDto.Name,
                Description = competenceDto.Description,
                Direction = competenceDto.Direction,
                Category = competenceDto.Category,
                Soft = competenceDto.Soft,
                Tasks = competenceDto.Tasks,
                ImageUrl = imagePath,
                TypeId = competenceDto.TypeId,
            };

            _context.Competences.Add(competence);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCompetence), new { id = competence.Id }, new
            {
                competence.Id,
                competence.Name,
                competence.Description,
                competence.Direction,
                competence.Category,
                competence.Soft,
                competence.Tasks,
                competence.TypeId,
                ImageUrl = GetImageUrl(competence.ImageUrl)
            });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompetence(int id, [FromForm] CompetenceDto competenceDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingCompetence = await _context.Competences.FindAsync(id);
            if (existingCompetence == null)
                return NotFound("Компетенция не найдена.");
            
            if (_context.Banners.Any(x => x.Title == competenceDto.Name && x.Id != id))
                return BadRequest("С таким названием компетенция уже существует.");

            if (competenceDto.ImageFile != null && competenceDto.ImageFile.Length > 0)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(existingCompetence.ImageUrl))
                {
                    var oldFilePath = Path.Combine(_env.WebRootPath, existingCompetence.ImageUrl);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Save new image
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(competenceDto.ImageFile.FileName);
                var filePath = Path.Combine(_competenceImagesPath, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await competenceDto.ImageFile.CopyToAsync(stream);
                }
                
                existingCompetence.ImageUrl = Path.Combine("uploads/competences", fileName);
            }

            if (competenceDto.TypeId == null)
                competenceDto.TypeId = 1;

            existingCompetence.Name = competenceDto.Name;
            existingCompetence.Description = competenceDto.Description;
            existingCompetence.Direction = competenceDto.Direction;
            existingCompetence.Category = competenceDto.Category;
            existingCompetence.Soft = competenceDto.Soft;
            existingCompetence.Tasks = competenceDto.Tasks;
            existingCompetence.TypeId = competenceDto.TypeId;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    existingCompetence.Id,
                    existingCompetence.Name,
                    existingCompetence.Description,
                    existingCompetence.Direction,
                    existingCompetence.Category,
                    existingCompetence.Soft,
                    existingCompetence.Tasks,
                    existingCompetence.TypeId,
                    ImageUrl = GetImageUrl(existingCompetence.ImageUrl)
                });
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
        
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompetence(int id)
        {
            var competence = await _context.Competences.FindAsync(id);
            if (competence == null)
            {
                return NotFound();
            }
            if (!string.IsNullOrEmpty(competence.ImageUrl))
            {
                var filePath = Path.Combine(_env.WebRootPath, competence.ImageUrl);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            _context.Competences.Remove(competence);
            await _context.SaveChangesAsync();
            return Ok();
        }
        
        private string GetImageUrl(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return null;
        
            var request = HttpContext.Request;
            return $"{request.Scheme}://{request.Host}/uploads/competences/{Path.GetFileName(imagePath)}";
        }
    }
}