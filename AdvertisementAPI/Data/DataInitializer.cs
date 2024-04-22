using AdvertisementAPI.Models;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace AdvertisementAPI.Data
{
    public class DataInitializer
    {
        private readonly AdContext _context;
        public DataInitializer(AdContext context)
        {
            _context = context;
        }
        public void MigrateData()
        {
            _context.Database.Migrate();
            SeedData();
            _context.SaveChanges();
        }
        public void SeedData()
        {
            _context.Database.EnsureCreated();

            if (_context.Ads.Any())
            {
                return;
            }

            var faker = new Faker<Ad>()
                .RuleFor(a => a.Title, f => Truncate(f.Commerce.ProductName(), 50))
                .RuleFor(a => a.Description, f => Truncate(f.Commerce.ProductAdjective() + ", " + f.Commerce.ProductDescription(), 500));




            var ads = faker.Generate(200);

            foreach (Ad ad in ads)
            {
                _context.Ads.Add(ad);
            }
        }

        private string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

    }
}
