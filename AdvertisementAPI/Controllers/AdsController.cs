using AdvertisementAPI.Data;
using AdvertisementAPI.Models;
using AdvertisementAPI.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AdvertisementAPI.Controllers
{
    /// <summary>
    /// Controller for ads
    /// </summary>
    /// <param name="context"></param>
    /// <param name="configuration"></param>
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class AdsController(AdContext context, IConfiguration configuration) : ControllerBase
    {
        /// <summary>
        /// Get all ads
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ad>>> GetAds()
        {
            return await context.Ads.ToListAsync();
        }

        /// <summary>
        /// Get a specific ad
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Ad>> GetAd(int id)
        {
            var ad = await context.Ads.FindAsync(id);

            if (ad == null)
            {
                return NotFound();
            }

            return Ok(ad);
        }

        /// <summary>
        /// Create a new ad
        /// </summary>
        /// <param name="adInput"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Ad>> PostAd(AdInputModel adInput)
        {
            var ad = new Ad
            {
                Title = adInput.Title,
                Description = adInput.Description
            };

            context.Ads.Add(ad);
            await context.SaveChangesAsync();

            return CreatedAtAction("GetAd", new { id = ad.Id }, ad);
        }

        /// <summary>
        /// Update an ad
        /// </summary>
        /// <param name="id"></param>
        /// <param name="adInput"></param>
        /// <returns></returns>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutAd(int id, AdInputModel adInput)
        {
            var ad = await context.Ads.FindAsync(id);

            if (ad == null)
            {
                return NotFound();
            }

            ad.Title = adInput.Title;
            ad.Description = adInput.Description;

            try
            {
                await context.SaveChangesAsync();
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

        /// <summary>
        /// Patch an ad
        /// </summary>
        /// <remarks>
        /// Sample requests:
        ///
        ///     Replace the Title property:
        ///     PATCH /api/ads/1
        ///     [
        ///         {
        ///             "op": "replace",
        ///             "path": "/Title",
        ///             "value": "New Title"
        ///         }
        ///     ]
        ///
        ///     Replace the Description property:
        ///     PATCH /api/ads/1
        ///     [
        ///         {
        ///             "op": "replace",
        ///             "path": "/Description",
        ///             "value": "New Description"
        ///         }
        ///     ]
        ///
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="patchDoc"></param>
        /// <returns></returns>
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> PatchAd(int id, JsonPatchDocument<Ad> patchDoc)
        {
            if (patchDoc != null)
            {
                var ad = await context.Ads.FindAsync(id);

                if (ad == null)
                {
                    return NotFound();
                }

                patchDoc.ApplyTo(ad, error =>
                {
                    string errorMessage = error.ErrorMessage;
                    string affectedPath = error.AffectedObject.GetType().Name;
                    ModelState.AddModelError(affectedPath, errorMessage);
                });

                if (!ModelState.IsValid)
                {
                    return new BadRequestObjectResult(ModelState);
                }

                context.Ads.Update(ad);
                context.Entry(ad).State = EntityState.Modified;

                try
                {
                    await context.SaveChangesAsync();
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
            else
            {
                return BadRequest();
            }
        }


        /// <summary>
        /// Delete an ad
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAd(int id)
        {
            var ad = await context.Ads.FindAsync(id);
            if (ad == null)
            {
                return NotFound();
            }

            context.Ads.Remove(ad);
            await context.SaveChangesAsync();

            return Ok();
        }

        private bool AdExists(int id)
        {
            return context.Ads.Any(e => e.Id == id);
        }

        /// <summary>
        /// Login to retrieve JWT token (execute and copy JWT token from response body)
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(LoginModel login)
        {
            if (string.IsNullOrEmpty(login.Username) || string.IsNullOrEmpty(login.Password))
            {
                return Unauthorized();
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"] ?? string.Empty);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, login.Username)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = configuration["Jwt:Issuer"],
                Audience = configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            return Ok(new LoginResult { Token = jwtToken });
        }
    }
}
