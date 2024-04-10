using System.Text.Json.Serialization;

namespace AdvertisementAPI.Models
{
    public class Ad
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
