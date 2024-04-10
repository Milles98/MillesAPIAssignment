using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AdvertisementAPI.Models
{
    public class Ad
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = null!;
        [Required]
        public string Description { get; set; } = null!;
    }
}
