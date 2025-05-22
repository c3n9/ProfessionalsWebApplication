using ProfessionalsWebApplication.Controllers;

namespace ProfessionalsWebApplication.Models.VK
{
    public class VkPhotosResponse
    {
        public int Count { get; set; }
        public List<VkPhotoItem> Items { get; set; }
    }
}
