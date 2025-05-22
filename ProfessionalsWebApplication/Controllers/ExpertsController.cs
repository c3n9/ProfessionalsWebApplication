using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProfessionalsWebApplication.Models;
using ProfessionalsWebApplication.Models.DTO;
using ProfessionalsWebApplication.Models.Settings;

namespace ProfessionalsWebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpertsController : Controller
    {
        private readonly ProfessionalsDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly string _expertImagesPath;

        public ExpertsController(ProfessionalsDbContext context,
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
            var imagesPath = fileStorageSettings?.Value?.ExpertImagesPath ?? "uploads/experts";
            // Комбинируем пути
            _expertImagesPath = Path.Combine(webRoot, imagesPath);
            // Создаем целевую директорию
            Directory.CreateDirectory(_expertImagesPath);
        }

        [HttpGet]
        public async Task<IActionResult> GetExperts()
        {
            var experts = await _context.Experts.ToListAsync();

            var result = experts.Select(x => new
            {
                x.Id,
                x.FullName,
                x.Post,
                x.CompetenceId,
                ImageUrl = GetImageUrl(x.ImageUrl)
            });

            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateExpert([FromForm] ExpertDto expertDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string imagePath = null;
            if (expertDto.ImageFile != null && expertDto.ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(expertDto.ImageFile.FileName);
                var filePath = Path.Combine(_expertImagesPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await expertDto.ImageFile.CopyToAsync(stream);
                }

                imagePath = Path.Combine("uploads/experts", fileName);
            }
            else
            {
                imagePath = "uploads/experts/no-photo.png";
            }

            var expert = new Expert()
            {
                FullName = expertDto.FullName,
                CompetenceId = expertDto.CompetenceId,
                Post = expertDto.Post,
                ImageUrl = imagePath,
            };

            _context.Experts.Add(expert);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetExperts), new { id = expert.Id }, new
            {
                expert.Id,
                expert.FullName,
                expert.Post,
                expert.CompetenceId,
                ImageUrl = GetImageUrl(expert.ImageUrl)
            });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExpert(int id, [FromForm] ExpertDto expertDto)
        {
            var existingExpert = await _context.Experts.FindAsync(id);
            if (existingExpert == null)
                return NotFound("Эксперт не найден.");

            if (expertDto.ImageFile != null && expertDto.ImageFile.Length > 0)
            {
                // Определяем имя файла
                string fileName;
                bool isNewFile = true;

                // Если у эксперта уже есть фото (и это не дефолтное), используем то же имя (но меняем расширение)
                if (!string.IsNullOrEmpty(existingExpert.ImageUrl))
                {
                    var currentFileNameWithoutExt = Path.GetFileNameWithoutExtension(existingExpert.ImageUrl);
                    if (!currentFileNameWithoutExt.Equals("no-photo", StringComparison.OrdinalIgnoreCase))
                    {
                        fileName = currentFileNameWithoutExt + Path.GetExtension(expertDto.ImageFile.FileName);
                        isNewFile = false;
                    }
                    else
                    {
                        fileName = Guid.NewGuid().ToString() + Path.GetExtension(expertDto.ImageFile.FileName);
                    }
                }
                else
                {
                    fileName = Guid.NewGuid().ToString() + Path.GetExtension(expertDto.ImageFile.FileName);
                }

                var filePath = Path.Combine(_expertImagesPath, fileName);

                // Удаляем старый файл, если он существует (кроме no-photo.png)
                if (!isNewFile)
                {
                    var oldFilePath = Path.Combine(_env.WebRootPath, existingExpert.ImageUrl);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Сохраняем файл
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await expertDto.ImageFile.CopyToAsync(stream);
                }

                existingExpert.ImageUrl = Path.Combine("uploads/experts", fileName);
            }
            // Если изображение не предоставлено - оставляем текущее

            existingExpert.FullName = expertDto.FullName;
            existingExpert.Post = expertDto.Post;
            existingExpert.CompetenceId = expertDto.CompetenceId;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    existingExpert.Id,
                    existingExpert.FullName,
                    existingExpert.Post,
                    existingExpert.CompetenceId,
                    ImageUrl = GetImageUrl(existingExpert.ImageUrl)
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                var entry = _context.Entry(existingExpert);
                await entry.ReloadAsync();
                if (entry.State == EntityState.Detached)
                    return NotFound("Эксперт был удалён.");
                else
                    return Conflict("Конфликт версий. Данные были изменены другим пользователем.");
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpert(int id)
        {
            var expert = await _context.Experts.FindAsync(id);
            if (expert == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(expert.ImageUrl))
            {
                var isDefaultImage = expert.ImageUrl.EndsWith("no-photo.png", StringComparison.OrdinalIgnoreCase);
                if (!isDefaultImage)
                {
                    var filePath = Path.Combine(_env.WebRootPath, expert.ImageUrl);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }

            _context.Experts.Remove(expert);
            await _context.SaveChangesAsync();
            return Ok();
        }


        private string GetImageUrl(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return null;

            var request = HttpContext.Request;
            return $"{request.Scheme}://{request.Host}/uploads/experts/{Path.GetFileName(imagePath)}";
        }
    }
}