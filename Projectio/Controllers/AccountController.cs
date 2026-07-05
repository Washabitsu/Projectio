using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Projectio.Core.Dtos;
using Projectio.Core.Models;
using Projectio.Persistence;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace Projectio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequireHttps]
    public class AccountController : ControllerBase
    {
        private IMapper _mapper { get; set; }
        private ApplicationDbContext _context { get; set; }
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly UserManager<ApplicationUser> _userManager;
        public AccountController(ApplicationDbContext context, IMapper mapper, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get()
        {
            var username = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username))
                return Unauthorized("Invalid token");

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return NotFound("User not found");

            var dto = _mapper.Map<ApplicationUser, UserOutDTO>(user);
            var roles = await _userManager.GetRolesAsync(user);
            
            if (roles.Count > 0)
                dto.Role = roles[0];

            return Ok(dto);
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Put([FromBody] UserInDTO value)
        {
            try
            {
                 if(!ModelState.IsValid)
                    return BadRequest(ModelState);

                var username = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(username))
                    return Unauthorized("Invalid token");

                var user = await _userManager.FindByNameAsync(username);
                if (user == null)
                    return NotFound("User not found");

                user.UpdateUser(value);

                if (!string.IsNullOrEmpty(value.Password))
                {
                    if (string.IsNullOrEmpty(value.CurrentPassword))
                        return BadRequest("Current password is required");

                    var passwordResult = await _userManager.ChangePasswordAsync(user, value.CurrentPassword, value.Password);

                    if (!passwordResult.Succeeded)
                        return BadRequest(passwordResult.Errors);
                }

                await _context.SaveChangesAsync();
                return Ok("User has been updated!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> Delete()
        {
            try
            {
                var username = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(username))
                    return Unauthorized("Invalid token");

                var user = await _userManager.FindByNameAsync(username);
                if (user == null)
                    return NotFound("User not found");

                _context.Users.Remove(user);

                await _context.SaveChangesAsync();
                return Ok("User has been deleted!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
