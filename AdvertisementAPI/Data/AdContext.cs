using AdvertisementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AdvertisementAPI.Data
{
    public class AdContext : DbContext
    {
        public AdContext(DbContextOptions<AdContext> options) : base(options)
        {
        }
        public DbSet<Ad> Ads { get; set; }
    }
}
