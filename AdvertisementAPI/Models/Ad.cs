using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AdvertisementAPI.Models
{
    /// <summary>
    /// Ad model
    /// </summary>
    public class Ad
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Title
        /// </summary>
        [Required]
        [StringLength(15)]
        public string Title { get; set; } = null!;
        /// <summary>
        /// Description
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Description { get; set; } = null!;
    }
}
