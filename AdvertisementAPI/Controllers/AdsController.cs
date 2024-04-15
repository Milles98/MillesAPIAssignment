using AdvertisementAPI.Data;
using AdvertisementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AdvertisementAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AdsController : ControllerBase
    {
        private readonly AdContext _context;
        private readonly IConfiguration _configuration;

        public AdsController(AdContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        //Get all ads, api/ads
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ad>>> GetAds()
        {
            return await _context.Ads.ToListAsync();
        }

        //Get ad by id, example api/ads/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Ad>> GetAd(int id)
        {
            var ad = await _context.Ads.FindAsync(id);

            if (ad == null)
            {
                return NotFound();
            }

            return Ok(ad);
        }

        //Create ad, api/ads
        [HttpPost]
        public async Task<ActionResult<Ad>> PostAd(AdInputModel adInput)
        {
            var ad = new Ad
            {
                Title = adInput.Title,
                Description = adInput.Description
            };

            _context.Ads.Add(ad);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAd", new { id = ad.Id }, ad);
        }

        //Update ad, api/ads/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAd(int id, AdInputModel adInput)
        {
            var ad = await _context.Ads.FindAsync(id);

            if (ad == null)
            {
                return NotFound();
            }

            ad.Title = adInput.Title;
            ad.Description = adInput.Description;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AdExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok();
        }

        // Partial update ad, api/ads/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchAd(int id, JsonPatchDocument<AdInputModel> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            var ad = await _context.Ads.FindAsync(id);

            if (ad == null)
            {
                return NotFound();
            }

            var adToPatch = new AdInputModel
            {
                Title = ad.Title,
                Description = ad.Description
            };

            patchDoc.ApplyTo(adToPatch, error =>
            {
                string path = error.AffectedObject.GetType().Name;
                ModelState.AddModelError(path, error.ErrorMessage);
            });


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ad.Title = adToPatch.Title;
            ad.Description = adToPatch.Description;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AdExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        //Delete ad, api/ads/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAd(int id)
        {
            var ad = await _context.Ads.FindAsync(id);
            if (ad == null)
            {
                return NotFound();
            }

            _context.Ads.Remove(ad);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool AdExists(int id)
        {
            return _context.Ads.Any(e => e.Id == id);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login(LoginModel login)
        {
            if (string.IsNullOrEmpty(login.Username) || string.IsNullOrEmpty(login.Password))
            {
                return Unauthorized();
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]); // Your secret key
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, login.Username)
                }),
                Expires = DateTime.UtcNow.AddDays(7), // Token validity period
                Issuer = _configuration["Jwt:Issuer"], // Setting the Issuer
                Audience = _configuration["Jwt:Audience"], // Setting the Audience to match the Issuer for simplicity
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            return Ok(new LoginResult { Token = jwtToken });
        }
    }
}
