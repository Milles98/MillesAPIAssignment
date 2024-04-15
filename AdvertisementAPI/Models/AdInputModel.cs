namespace AdvertisementAPI.Models
{
    /// <summary>
    /// Input model for creating an ad
    /// </summary>
    public class AdInputModel
    {
        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; } = null!;
        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; } = null!;
    }
}
