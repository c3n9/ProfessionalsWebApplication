using ProfessionalsWebApplication.Controllers;

namespace ProfessionalsWebApplication.Models.VK
{
    public class VkPhotoItem
    {
        public long Id { get; set; }
        public List<VkPhotoSize> Sizes { get; set; }
    }
}
