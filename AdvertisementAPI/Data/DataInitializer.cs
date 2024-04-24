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
            SeedUsers();
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
                .RuleFor(a => a.Description, f => Truncate(f.Commerce.ProductAdjective() + ", " + f.Commerce.ProductDescription(), 500))
                .RuleFor(a => a.Price, f => f.Random.Decimal(1.0m, 1000.0m))
                .RuleFor(a => a.DateAdded, f => DateOnly.FromDateTime(f.Date.Past(10)))
                .RuleFor(a => a.IsDeleted, f => f.Random.Int(0, 1));

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

        public void SeedUsers()
        {
            if (_context.AdUsers.Any())
            {
                return;
            }

            var user = new AdUser
            {
                Username = "AdsUser",
                Password = "AdsUserPassword123!",
                Role = "User"
            };
            _context.AdUsers.Add(user);

            var admin = new AdUser
            {
                Username = "AdsAdmin",
                Password = "AdsAdminPassword123!",
                Role = "Admin"
            };
            _context.AdUsers.Add(admin);

            _context.SaveChanges();
        }


    }
}
