using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfessionalsWebApplication.Models;
using ProfessionalsWebApplication.Models.DTO;
using Microsoft.Extensions.Options;
using System.IO;
using ProfessionalsWebApplication.Models.Settings;

namespace ProfessionalsWebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BannersController : ControllerBase
    {
        private readonly ProfessionalsDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly string _bannerImagesPath;

        public BannersController(
            ProfessionalsDbContext context, 
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
            var imagesPath = fileStorageSettings?.Value?.BannerImagesPath ?? "uploads/banners";
    
            // Комбинируем пути
            _bannerImagesPath = Path.Combine(webRoot, imagesPath);
    
            // Создаем целевую директорию
            Directory.CreateDirectory(_bannerImagesPath);
        }

        [HttpGet]
        public async Task<IActionResult> GetBanners()
        {
            var banners = await _context.Banners.ToListAsync();
            
            // Convert image paths to URLs
            var result = banners.Select(b => new
            {
                b.Id,
                b.Title,
                b.Description,
                b.LinkToButton,
                ImageUrl = GetImageUrl(b.ImagePath)
            });
            
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBanner([FromForm] BannerDto bannerDto)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            if (_context.Banners.Any(x => x.Title == bannerDto.Title))
                return BadRequest("С таким названием баннер уже существует.");

            // Handle file upload
            string imagePath = null;
            if (bannerDto.ImageFile != null && bannerDto.ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(bannerDto.ImageFile.FileName);
                var filePath = Path.Combine(_bannerImagesPath, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await bannerDto.ImageFile.CopyToAsync(stream);
                }
                
                imagePath = Path.Combine("uploads/banners", fileName); // Relative path
            }

            var banner = new Banner
            {
                Title = bannerDto.Title,
                Description = bannerDto.Description,
                ImagePath = imagePath,
                LinkToButton = bannerDto.LinkToButton
            };

            _context.Banners.Add(banner);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBanners), new { id = banner.Id }, new
            {
                banner.Id,
                banner.Title,
                banner.Description,
                banner.LinkToButton,
                ImageUrl = GetImageUrl(banner.ImagePath)
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBanner(int id, [FromForm] BannerDto bannerDto)
        {
            var existingBanner = await _context.Banners.FindAsync(id);
            if (existingBanner == null)
                return NotFound("Баннер не найден.");

            if (_context.Banners.Any(x => x.Title == bannerDto.Title && x.Id != id))
                return BadRequest("С таким названием баннер уже существует.");

            // Handle file upload if a new image is provided
            if (bannerDto.ImageFile != null && bannerDto.ImageFile.Length > 0)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(existingBanner.ImagePath))
                {
                    var oldFilePath = Path.Combine(_env.WebRootPath, existingBanner.ImagePath);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Save new image
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(bannerDto.ImageFile.FileName);
                var filePath = Path.Combine(_bannerImagesPath, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await bannerDto.ImageFile.CopyToAsync(stream);
                }
                
                existingBanner.ImagePath = Path.Combine("uploads/banners", fileName);
            }

            // Update other fields
            existingBanner.Title = bannerDto.Title;
            existingBanner.Description = bannerDto.Description;
            existingBanner.LinkToButton = bannerDto.LinkToButton;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    existingBanner.Id,
                    existingBanner.Title,
                    existingBanner.Description,
                    existingBanner.LinkToButton,
                    ImageUrl = GetImageUrl(existingBanner.ImagePath)
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                var entry = _context.Entry(existingBanner);
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
            if (!string.IsNullOrEmpty(banner.ImagePath))
            {
                var filePath = Path.Combine(_env.WebRootPath, banner.ImagePath);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Banners.Remove(banner);
            await _context.SaveChangesAsync();
            return Ok();
        }

        private string GetImageUrl(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return null;
        
            var request = HttpContext.Request;
            return $"{request.Scheme}://{request.Host}/uploads/banners/{Path.GetFileName(imagePath)}";
        }
    }
}