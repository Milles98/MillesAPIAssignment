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
    //[Authorize]
    [Route("[controller]")]
    [ApiController]
    public class AdsController(AdContext context, IConfiguration configuration) : ControllerBase
    {
        /// <summary>
        /// Get all ads
        /// </summary>
        /// <remarks>
        /// Type active or deleted in the status parameter to filter the ads by status. Otherwise leave empty to see all ads.
        /// </remarks>
        /// <return>
        /// A list of all ads
        /// </return>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ad>>> GetAds([FromQuery] string status = "")
        {
            IQueryable<Ad> query = context.Ads;

            if (!string.IsNullOrEmpty(status))
            {
                if (status.ToLower() == "active")
                {
                    query = query.Where(a => a.IsDeleted == 0);
                }
                else if (status.ToLower() == "deleted")
                {
                    query = query.Where(a => a.IsDeleted == 1);
                }
            }

            return await query.ToListAsync();
        }

        /// <summary>
        /// Get a specific ad
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// A specific ad
        /// </returns>
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
        /// Create a new ad (Admin &amp; User)
        /// </summary>
        /// <param name="adInput"></param>
        /// <returns>
        /// A newly created ad
        /// </returns>
        [HttpPost]
        [Authorize(Roles = "Admin, User")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<Ad>> PostAd(AdPostDTO adInput)
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

            return CreatedAtAction("GetAd", new { id = ad.Id }, ad);
            //return Ok(ad);
        }


        /// <summary>
        /// Update an ad (Admin &amp; User) Can also be used to soft delete an ad
        /// </summary>
        /// <remarks>
        /// IsDeleted functionality is only available for Admins. Users can only update the Title, Description, and Price properties.
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="adInput"></param>
        /// <returns></returns>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> PutAd(int id, AdDto adInput)
        {
            var ad = await context.Ads.FindAsync(id);

            if (ad == null)
            {
                return NotFound();
            }

            if (adInput.IsDeleted == 1)
            {
                if (!User.IsInRole("Admin"))
                {
                    return Forbid();
                }
            }

            ad.Title = adInput.Title;
            ad.Description = adInput.Description;
            ad.Price = adInput.Price;
            ad.DateAdded = DateOnly.FromDateTime(DateTime.Today);
            ad.IsDeleted = adInput.IsDeleted;

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
        /// Patch an ad (Admin &amp; User)
        /// </summary>
        /// <remarks>
        /// Sample requests:
        ///
        ///     Replace the Title property:
        ///     [
        ///         {
        ///             "path": "/Title",
        ///             "op": "replace",
        ///             "value": "Type new title here"
        ///         }
        ///     ]
        ///
        ///     Replace the Description property:
        ///     [
        ///         {
        ///             "path": "/Description",
        ///             "op": "replace",
        ///             "value": "Type new description here"
        ///         }
        ///     ]
        ///     
        ///     Replace the Price property:
        ///     [
        ///         {
        ///             "path": "/Price",
        ///             "op": "replace",
        ///             "value": "Example: 399"
        ///         }
        ///     ]
        ///     
        /// Note: The "operationType" and "from" fields are part of the JSON Patch standard but are not used in this API. Please do not include them in your requests.
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="patchDoc"></param>
        /// <returns>
        /// A partially updated ad
        /// </returns>
        [HttpPatch("{id:int}")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> PatchAd(int id, JsonPatchDocument<AdDto> patchDoc)
        {
            if (patchDoc != null)
            {
                var ad = await context.Ads.FindAsync(id);

                if (ad == null)
                {
                    return BadRequest("Ad not found");
                }

                var adDto = new AdDto
                {
                    Title = ad.Title,
                    Description = ad.Description,
                    Price = ad.Price
                };

                patchDoc.ApplyTo(adDto, error =>
                {
                    string errorMessage = error.ErrorMessage;
                    string affectedPath = error.AffectedObject.GetType().Name;
                    ModelState.AddModelError(affectedPath, errorMessage);
                });

                if (!ModelState.IsValid)
                {
                    return new BadRequestObjectResult(ModelState);
                }

                ad.Title = adDto.Title;
                ad.Description = adDto.Description;
                ad.Price = adDto.Price;

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
        /// <returns>
        /// A deleted ad
        /// </returns>
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

        private bool AdExists(int id)
        {
            return context.Ads.Any(e => e.Id == id);
        }

    }
}
