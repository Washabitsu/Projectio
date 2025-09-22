using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Projectio.Core.Dtos;
using Projectio.Core.Interfaces;
using Projectio.Core.Models;
using Projectio.Persistence;


namespace Projectio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private IJWT _jwt { get; set; }
        private IMapper _mapper { get; set; }
        private ApplicationDbContext _context { get; set; }
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly UserManager<ApplicationUser> _userManager;
        public AccountController(ApplicationDbContext context, IJWT jwt, IMapper mapper, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _mapper = mapper;
            _jwt = jwt;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get()
        {
            var user = HttpContext.Items["CurrentUser"] as ApplicationUser;

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
                ApplicationUser user = HttpContext.Items["CurrentUser"] as ApplicationUser;

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
                ApplicationUser user = HttpContext.Items["CurrentUser"] as ApplicationUser; ;

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
