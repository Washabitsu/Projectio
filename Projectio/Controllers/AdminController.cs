using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Projectio.Core.Dtos;
using Projectio.Core.Models;
using Projectio.Persistence;

namespace Projectio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequireHttps]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }


        [HttpPost("UpdateRoles/{userid}")]

        public async Task<IActionResult> UpdateRoles(string userid,[FromBody] RoleInDto value)
        {

            if (string.IsNullOrWhiteSpace(userid))
                return BadRequest("User id is required.");

            var user = await _userManager.FindByIdAsync(userid);
            if (user == null)
                return NotFound("User not found.");

            if (string.IsNullOrWhiteSpace(value?.Name))
                return BadRequest("Role name is required.");

            var role = await _roleManager.RoleExistsAsync(value.Name);
           

            if (!role)
                return BadRequest("The role does not exist");
            

            var current_roles = await _userManager.GetRolesAsync(user); 

            try
            {
                await _userManager.RemoveFromRolesAsync(user, current_roles);
                var result_role_assignment = await _userManager.AddToRoleAsync(user, value.Name);

                if (!result_role_assignment.Succeeded)
                    return BadRequest("Error assigning role to user");

                return Ok("The user has been created succefully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }
    }

}
