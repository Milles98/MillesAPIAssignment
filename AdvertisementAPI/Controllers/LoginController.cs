using AdvertisementAPI.Data;
using AdvertisementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AdvertisementAPI.Controllers
{
    [EnableCors("AllowAll")]
    [Route("[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly AdContext context;
        private readonly IConfiguration configuration;

        public LoginController(AdContext context, IConfiguration configuration)
        {
            this.context = context;
            this.configuration = configuration;
        }

        /// <summary>
        /// Login to retrieve JWT token
        /// </summary>
        /// <remarks>
        /// Available logins for testing:
        /// 
        ///     Admin:
        ///         {
        ///             "username": "AdsAdmin",
        ///             "password": "AdsAdminPassword123!",
        ///         }
        ///         
        ///     User:
        ///         {
        ///             "username": "AdsUser",
        ///             "password": "AdsUserPassword123!",
        ///         }
        /// </remarks>
        /// <param name="login"></param>
        /// <returns>
        /// A JWT token
        /// </returns>
        [AllowAnonymous]
        [HttpPost]
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

            return Ok(jwtToken);
        }
    }
}
