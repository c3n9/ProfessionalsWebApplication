using System.ComponentModel.DataAnnotations;

namespace ProfessionalsWebApplication.Models.DTO
{
    public class VKAlbumDto
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string? Url { get; set; }
        [Required]
        public int Year { get; set; }

    }
}
