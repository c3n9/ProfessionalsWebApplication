using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProfessionalsWebApplication.Models;
using ProfessionalsWebApplication.Models.DTO;
using ProfessionalsWebApplication.Models.Settings;

namespace ProfessionalsWebApplication.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CompetitorsController : Controller
{
    private readonly ProfessionalsDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly string _competitorImagesPath;

    public CompetitorsController(ProfessionalsDbContext context,
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
        var imagesPath = fileStorageSettings?.Value?.CompetitorImagesPath ?? "uploads/competitors";
        // Комбинируем пути
        _competitorImagesPath = Path.Combine(webRoot, imagesPath);
        // Создаем целевую директорию
        Directory.CreateDirectory(_competitorImagesPath);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetCompetitors()
    {
        var competitors = await _context.Competitors.ToListAsync();
            
        // Convert image paths to URLs
        var result = competitors.Select(b => new
        {
            b.Id,
            b.FullName,
            b.CompetenceId,
            b.ChampionshipId,
            b.Place,
            ImageUrl = GetImageUrl(b.ImageUrl)
        });
            
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateBanner([FromForm] CompetitorDto сompetitorDto)
    {
        if (!ModelState.IsValid) 
            return BadRequest(ModelState);

        string imagePath = null;
        if (сompetitorDto.ImageFile != null && сompetitorDto.ImageFile.Length > 0)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(сompetitorDto.ImageFile.FileName);
            var filePath = Path.Combine(_competitorImagesPath, fileName);
                
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await сompetitorDto.ImageFile.CopyToAsync(stream);
            }
                
            imagePath = Path.Combine("uploads/competitors", fileName);
        }

        var competitor = new Competitor()
        {
            FullName = сompetitorDto.FullName,
            CompetenceId = сompetitorDto.CompetenceId,
            ChampionshipId = сompetitorDto.ChampionshipId,
            Place = сompetitorDto.Place,
            ImageUrl = imagePath,
        };

        _context.Competitors.Add(competitor);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCompetitors), new { id = competitor.Id }, new
        {
            competitor.Id,
            competitor.FullName,
            competitor.CompetenceId,
            competitor.ChampionshipId,
            competitor.Place,
            ImageUrl = GetImageUrl(competitor.ImageUrl)
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCompetitor(int id, [FromForm] CompetitorDto bannerDto)
    {
        var existingCompetitor = await _context.Competitors.FindAsync(id);
        if (existingCompetitor == null)
            return NotFound("Участник не найден.");

        if (bannerDto.ImageFile != null && bannerDto.ImageFile.Length > 0)
        {
            if (!string.IsNullOrEmpty(existingCompetitor.ImageUrl))
            {
                var oldFilePath = Path.Combine(_env.WebRootPath, existingCompetitor.ImageUrl);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            // Save new image
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(bannerDto.ImageFile.FileName);
            var filePath = Path.Combine(_competitorImagesPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await bannerDto.ImageFile.CopyToAsync(stream);
            }

            existingCompetitor.ImageUrl = Path.Combine("uploads/competitors", fileName);
        }

        existingCompetitor.FullName = bannerDto.FullName;
        existingCompetitor.CompetenceId = bannerDto.CompetenceId;
        existingCompetitor.ChampionshipId = bannerDto.ChampionshipId;
        existingCompetitor.Place = bannerDto.Place;

        try
        {
            await _context.SaveChangesAsync();
            return Ok(new
            {
                existingCompetitor.Id,
                existingCompetitor.FullName,
                existingCompetitor.ChampionshipId,
                existingCompetitor.Place,
                existingCompetitor.CompetenceId,
                ImageUrl = GetImageUrl(existingCompetitor.ImageUrl)
            });
        }
        catch (DbUpdateConcurrencyException)
        {
            var entry = _context.Entry(existingCompetitor);
            await entry.ReloadAsync();
            if (entry.State == EntityState.Detached)
                return NotFound("Участник был удалён.");
            else
                return Conflict("Конфликт версий. Данные были изменены другим пользователем.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCompetitor(int id)
    {
        var competitor = await _context.Competitors.FindAsync(id);
        if (competitor == null)
        {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(competitor.ImageUrl))
        {
            var filePath = Path.Combine(_env.WebRootPath, competitor.ImageUrl);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        _context.Competitors.Remove(competitor);
        await _context.SaveChangesAsync();
        return Ok();
    }


    private string GetImageUrl(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
            return null;

        var request = HttpContext.Request;
        return $"{request.Scheme}://{request.Host}/uploads/competitors/{Path.GetFileName(imagePath)}";
    }
}