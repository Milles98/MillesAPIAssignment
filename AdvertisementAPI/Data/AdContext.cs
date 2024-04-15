using AdvertisementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AdvertisementAPI.Data
{
    /// <summary>
    /// Context for ads
    /// </summary>
    /// <param name="options"></param>
    public class AdContext(DbContextOptions<AdContext> options) : DbContext(options)
    {
        /// <summary>
        /// Ads
        /// </summary>
        public DbSet<Ad> Ads { get; set; }
    }
}
