using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProfessionalsWebApplication.Models.VK;
using ProfessionalsWebApplication.Models;
using Microsoft.EntityFrameworkCore;
using ProfessionalsWebApplication.Models.DTO;
using System.Text.RegularExpressions;
using ProfessionalsWebApplication.Services;
using Microsoft.AspNetCore.Authorization;

namespace ProfessionalsWebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VKPhotosController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _vkAccessToken;
        private readonly ProfessionalsDbContext _context;

        public VKPhotosController(IHttpClientFactory httpClientFactory, IConfiguration configuration,
            ProfessionalsDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _vkAccessToken = configuration["VK:AccessToken"];
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> GetCompetitors()
        {
            var albums = await _context.VKAlbums.ToListAsync();

            var result = albums.Select(b => new
            {
                b.Id,
                b.Title,
                b.Year,
                Url = $"https://vk.com/album{b.OwnerId}_{b.AlbumId}"
            });

            return Ok(result);
        }

        [Authorize]
        [HttpPost]
		public async Task<IActionResult> CreateAlbum([FromForm] VKAlbumDto vkAlbumDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (isValid, ownerId, albumIdFromUrl) = ParseVKAlbumUrl(vkAlbumDto.Url);
            if (!isValid)
            {
                return BadRequest(
                    "Некорректная ссылка на альбом VK. Ожидается формат: https://vk.com/album-XXXXXX_YYYYYYY");
            }

            var vkAlbum = new VKAlbum
            {
                Title = vkAlbumDto.Title,
                Year = vkAlbumDto.Year,
                OwnerId = ownerId.ToString(),
                AlbumId = albumIdFromUrl.ToString()
            };


            _context.VKAlbums.Add(vkAlbum);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                vkAlbum.Id,
                vkAlbum.Title,
                vkAlbum.Year,
                Url = $"https://vk.com/album{vkAlbum.OwnerId}_{vkAlbum.AlbumId}"
            });
        }

        [Authorize]
        [HttpPut("{albumId}")]
		public async Task<IActionResult> UpdateVKAlbum(int albumId, [FromForm] VKAlbumDto vkAlbumDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (isValid, ownerId, albumIdFromUrl) = ParseVKAlbumUrl(vkAlbumDto.Url);
            if (!isValid)
            {
                return BadRequest(
                    "Некорректная ссылка на альбом VK. Ожидается формат: https://vk.com/album-XXXXXX_YYYYYYY");
            }

            var existingVKAlbum = await _context.VKAlbums.FindAsync(albumId);
            if (existingVKAlbum == null)
                return NotFound("Альбом не найден.");

            existingVKAlbum.Title = vkAlbumDto.Title;
            existingVKAlbum.OwnerId = ownerId.ToString();
            existingVKAlbum.AlbumId = albumIdFromUrl.ToString();
            existingVKAlbum.Year = vkAlbumDto.Year;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    existingVKAlbum.Id,
                    existingVKAlbum.Title,
                    existingVKAlbum.Year,
                    Url = $"https://vk.com/album{existingVKAlbum.OwnerId}_{existingVKAlbum.AlbumId}"
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                var entry = _context.Entry(existingVKAlbum);
                await entry.ReloadAsync();
                if (entry.State == EntityState.Detached)
                    return NotFound("Альбом был удалён.");
                else
                    return Conflict("Конфликт версий. Данные были изменены другим пользователем.");
            }
        }

        private (bool isValid, long ownerId, long albumId) ParseVKAlbumUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return (false, 0, 0);
            var match = Regex.Match(url, @"album-(\d+)_(\d+)");
            if (!match.Success)
                return (false, 0, 0);

            if (!long.TryParse(match.Groups[1].Value, out var ownerId) ||
                !long.TryParse(match.Groups[2].Value, out var albumId))
            {
                return (false, 0, 0);
            }

            return (true, -ownerId, albumId);
        }

        [Authorize]
        [HttpDelete("{id}")]
		public async Task<IActionResult> DeleteVKAlbum(int id)
        {
            var vkAlbum = await _context.VKAlbums.FindAsync(id);
            if (vkAlbum == null)
            {
                return NotFound();
            }

            _context.VKAlbums.Remove(vkAlbum);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("public/{vkAlbumId}")]
        public async Task<ActionResult<IEnumerable<VkPhotoResponse>>> GetPublicAlbumPhotos(int vkAlbumId)
        {
            try
            {
                var album = _context.VKAlbums.FirstOrDefault(x => x.Id == vkAlbumId);

                if (album == null)
                {
                    Response.Headers["Cache-Control"] = "no-store, no-cache";
                    Response.Headers["Pragma"] = "no-cache";
                    return NotFound("Альбом не найден");
                }

                string ownerId = album.OwnerId;
                string albumId = album.AlbumId;

                const int
                    count = 50; // Максимальное количество фотографий за один запрос (макс. 1000 для некоторых методов)
                int offset = 0;
                var allPhotos = new List<VkPhotoItem>();

                do
                {
                    var apiUrl = $"https://api.vk.com/method/photos.get?owner_id={ownerId}" +
                                 $"&album_id={albumId}" +
                                 $"&access_token={_vkAccessToken}" +
                                 $"&count={count}" +
                                 $"&offset={offset}" +
                                 "&extended=1" +
                                 "&photo_sizes=1" +
                                 "&v=5.131";

                    var client = _httpClientFactory.CreateClient();
                    var response = await client.GetFromJsonAsync<VkApiResponse>(apiUrl);

                    if (response?.Response?.Items == null || !response.Response.Items.Any())
                    {
                        break;
                    }

                    allPhotos.AddRange(response.Response.Items);
                    offset += count;

                    // Если получено меньше фотографий, чем запрошено, значит это последняя страница
                    if (response.Response.Items.Count < count)
                    {
                        break;
                    }

                    // Небольшая задержка, чтобы избежать лимитов VK API
                    await Task.Delay(500);
                } while (true);

                if (!allPhotos.Any())
                {
                    Response.Headers["Cache-Control"] = "no-store, no-cache";
                    Response.Headers["Pragma"] = "no-cache";
                    return NotFound(new
                    {
                        Message = "No photos found in the album",
                        DebugInfo = new
                        {
                            OwnerId = ownerId,
                            AlbumId = albumId
                        }
                    });
                }

                var photos = allPhotos
                    .SelectMany(photo => photo.Sizes
                        .Where(size => size.Type == "z")
                        .Select(size => new VkPhotoResponse
                        {
                            Url = size.Url,
                        }))
                    .ToList();
                // Устанавливаем кэширование только для успешного ответа
                Response.Headers["Cache-Control"] = "public, max-age=3600";

                return Ok(photos);
            }
            catch (HttpRequestException ex)
            {
                Response.Headers["Cache-Control"] = "no-store, no-cache";
                Response.Headers["Pragma"] = "no-cache";
                return StatusCode(502, new
                {
                    Error = "VK API request failed",
                    Details = ex.Message
                });
            }
            catch (Exception ex)
            {
                Response.Headers["Cache-Control"] = "no-store, no-cache";
                Response.Headers["Pragma"] = "no-cache";
                return StatusCode(500, new
                {
                    Error = "Internal server error",
                    Details = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("public/random")]
        public async Task<ActionResult<IEnumerable<VkPhotoResponse>>> GetPublicAlbumPhotos()
        {
            try
            {
                var albums = _context.VKAlbums.ToList();
                if (albums.Count == 0)
                {
                    return NotFound("Не найдено ни одного альбома");
                }
                Random rnd = new Random();
                var album = albums[rnd.Next(1, albums.Count)];
                if (album == null)
                {
                    return NotFound("Не найдено ни одного альбома");
                }

                string ownerId = album.OwnerId;
                string albumId = album.AlbumId;

                const int count = 20; // Запрашиваем сразу больше фотографий для выбора случайных
                const int returnCount = 5; // Сколько фотографий вернуть в ответе

                var apiUrl = $"https://api.vk.com/method/photos.get?owner_id={ownerId}" +
                             $"&album_id={albumId}" +
                             $"&access_token={_vkAccessToken}" +
                             $"&count={count}" +
                             "&extended=1" +
                             "&photo_sizes=1" +
                             "&v=5.131";

                var client = _httpClientFactory.CreateClient();
                var response = await client.GetFromJsonAsync<VkApiResponse>(apiUrl);

                if (response?.Response?.Items == null || !response.Response.Items.Any())
                {
                    return NotFound(new
                    {
                        Message = "No photos found in the album",
                        DebugInfo = new
                        {
                            OwnerId = ownerId,
                            AlbumId = albumId
                        }
                    });
                }

                // Выбираем случайные фотографии из полученных
                var randomPhotos = response.Response.Items
                    .OrderBy(x => rnd.Next())
                    .Take(returnCount)
                    .SelectMany(photo => photo.Sizes
                        .Where(size => size.Type == "z")
                        .Select(size => new VkPhotoResponse
                        {
                            Url = size.Url,
                        }))
                    .ToList();

                return Ok(randomPhotos);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new
                {
                    Error = "VK API request failed",
                    Details = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "Internal server error",
                    Details = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }
    }
}