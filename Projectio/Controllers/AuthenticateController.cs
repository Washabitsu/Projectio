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
        public async Task<ActionResult> Authenticate(LoginDto dto)
        {
            try
            {
                ApplicationUser user = await _context.Users.FirstOrDefaultAsync(i => i.UserName == dto.Username);
                if (user == null)
                    return NotFound();

                var roles = (await _userManager.GetRolesAsync(user)).ToList();

                var result =  await _userManager.CheckPasswordAsync(user, dto.Password);

                if (result)
                {
                    var claims = new List<Claim>();
                    foreach (var role in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                    var token = await _jwt.GetJwtToken(dto.Username, claims);
                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expires = token.ValidTo
                    });
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
