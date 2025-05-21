using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProfessionalsWebApplication.Models.VK;

namespace ProfessionalsWebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VKPhotosController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _vkAccessToken;

        public VKPhotosController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _vkAccessToken = configuration["VK:AccessToken"];
        }

        [HttpGet("public/{ownerId}/{albumId}")]
        //[ResponseCache(Duration = 3600)] // Кеширование на 1 час
        public async Task<ActionResult<IEnumerable<VkPhotoResponse>>> GetPublicAlbumPhotos(long ownerId, string albumId)
        {
            try
            {
                const int count = 50; // Максимальное количество фотографий за один запрос (макс. 1000 для некоторых методов)
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

                return Ok(photos);
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