﻿using AdvertisementAPI.Data;
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
    //[Authorize]
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
        /// Get all deleted ads
        /// </summary>
        /// <returns></returns>
        [HttpGet("DeletedAds")]
        public async Task<ActionResult<IEnumerable<Ad>>> GetDeletedAds()
        {
            return await context.Ads
                .Where(a => a.IsDeleted == 1)
                .ToListAsync();
        }

        /// <summary>
        /// Get all active ads
        /// </summary>
        /// <returns></returns>
        [HttpGet("ActiveAds")]
        public async Task<ActionResult<IEnumerable<Ad>>> GetActiveAds()
        {
            return await context.Ads
                .Where(a => a.IsDeleted == 0)
                .ToListAsync();
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
                return BadRequest("Ad not found");
            }

            return Ok(ad);
        }

        /// <summary>
        /// Create a new ad
        /// </summary>
        /// <param name="adInput"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Ad>> PostAd(AdDTO adInput)
        {
            var ad = new Ad
            {
                Title = adInput.Title,
                Description = adInput.Description,
                Price = adInput.Price,
                DateAdded = DateOnly.FromDateTime(DateTime.Today),
                IsDeleted = 0
            };

            context.Ads.Add(ad);
            await context.SaveChangesAsync();

            //return CreatedAtAction("GetAd", new { id = ad.Id }, ad);
            return Ok(ad);
        }

        /// <summary>
        /// Update an ad
        /// </summary>
        /// <param name="id"></param>
        /// <param name="adInput"></param>
        /// <returns></returns>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutAd(int id, AdDTO adInput)
        {
            var ad = await context.Ads.FindAsync(id);

            if (ad == null)
            {
                return NotFound();
            }

            ad.Title = adInput.Title;
            ad.Description = adInput.Description;
            ad.Price = adInput.Price;
            ad.DateAdded = DateOnly.FromDateTime(DateTime.Today);
            //ad.IsDeleted = 0;

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

            return Ok(ad);
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
        ///     Replace the Price property:
        ///     PATCH /api/ads/1
        ///     [
        ///         {
        ///             "op": "replace",
        ///             "path": "/Price",
        ///             "value": "New Price"
        ///         }
        ///     ]
        ///     
        ///     Replace the DateAdded property:
        ///     PATCH /api/ads/1
        ///     [
        ///         {
        ///             "op": "replace",
        ///             "path": "/DateAdded",
        ///             "value": "2024-05-25"
        ///         }
        ///     ]
        ///     
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
                    return BadRequest("Ad not found");
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

                return Ok(ad);
            }
            else
            {
                return BadRequest();
            }
        }


        /// <summary>
        /// Permanently delete an ad (Admin only)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAd(int id)
        {
            var ad = await context.Ads.FindAsync(id);
            if (ad == null)
            {
                return BadRequest("Ad not found");
            }

            context.Ads.Remove(ad);
            await context.SaveChangesAsync();

            return Ok(ad);
        }

        /// <summary>
        /// Soft delete an ad (Admin only)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("SoftDelete/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SoftDeleteAd(int id)
        {
            var ad = await context.Ads.FindAsync(id);
            if (ad == null)
            {
                return BadRequest("Ad not found");
            }

            ad.IsDeleted = 1;
            await context.SaveChangesAsync();

            return Ok(ad);
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
            var user = context.AdUsers.SingleOrDefault(u => u.Username == login.Username && u.Password == login.Password);

            if (user == null)
            {
                return Unauthorized();
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, login.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                Issuer = configuration["Jwt:Issuer"],
                Audience = configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            //return Ok(new LoginResult { Token = jwtToken });
            return Ok(jwtToken);
        }
    }
}
