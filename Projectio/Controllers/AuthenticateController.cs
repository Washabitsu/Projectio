using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Projectio.Core.Dtos;
using Projectio.Core.Interfaces;
using Projectio.Core.Models;
using Projectio.Helpers;
using Projectio.Persistence;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Projectio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly IJWT _jwt;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private IMapperWrapper _mapperWrapper { get; set; }

        public AuthenticateController(IJWT jwt, ApplicationDbContext context, IMapperWrapper mapperWrapper, UserManager<ApplicationUser> userManager)
        {
            _jwt = jwt;
            _mapperWrapper = mapperWrapper;
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<object> Authenticate(LoginDto dto)
        {
            try
            {
                ApplicationUser user = await _context.Users.FirstOrDefaultAsync(i => i.UserName == dto.Username);
                if (user == null)
                    return NotFound();

                List<string> roles = _userManager.GetRolesAsync(user).Result.ToList();


                bool verified = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
                if (verified)
                {
                    var claims = new List<Claim>();
                    foreach (var role in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                    var token = await _jwt.GetJwtToken(dto.Username, claims);
                    return new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expires = token.ValidTo
                    };
                }
                return Unauthorized("You are not authorized!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
