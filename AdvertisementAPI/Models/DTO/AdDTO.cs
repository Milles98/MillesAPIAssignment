namespace AdvertisementAPI.Models.DTO
{
    /// <summary>
    /// Input model for creating an ad
    /// </summary>
    public class AdDTO
    {
        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; } = null!;
        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; } = null!;

        //public decimal Price { get; set; }

        //public DateTime DateAdded { get; set; } 

        //public int IsDeleted { get; set; }
    }
}
