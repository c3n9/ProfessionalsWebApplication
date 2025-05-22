using System.ComponentModel.DataAnnotations;

namespace ProfessionalsWebApplication.Models
{
    public class VKAlbum
    {
        [Key] public int Id { get; set; }
        public string Title { get; set; }
        public string OwnerId { get; set; }
        public string AlbumId { get; set; }
        public int Year { get; set; }
    }
}
