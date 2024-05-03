using System.Text.Json.Serialization;

namespace AdvertisementAPI.Models.DTO
{
    public class AdPostDTO
    {
        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; } = null!;
        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; } = null!;

        public decimal Price { get; set; }

        [JsonIgnore]
        public int IsDeleted { get; set; }
    }
}
