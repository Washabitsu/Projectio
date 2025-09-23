using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Projectio.Core.Dtos;
using Projectio.Core.Models;
using Projectio.Persistence;
using Projectio.Security.Interfaces.JWT;

namespace Projectio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IJWT _jwt;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(IJWT jwt, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _jwt = jwt;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }


        [HttpPost("CreateUser")]

        public async Task<IActionResult> CreateUser([FromBody] UserRegisterDTO value)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(value.Password))
                return BadRequest("Password is required.");

            if (!string.IsNullOrWhiteSpace(value.Role))
            {
                var roleExists = await _roleManager.RoleExistsAsync(value.Role);
                if (!roleExists)
                    return BadRequest("The role does not exist.");
            }

            var user = new ApplicationUser
            {
                UserName = value.Username,
                Email = value.Email,
                EmailConfirmed = true
            };


            try
            {

                var result = await _userManager.CreateAsync(user, value.Password);
                var result_role_assignment = await _userManager.AddToRoleAsync(user, value.Role);

                if (!result.Succeeded)
                    return BadRequest("User couldn't be created");

                if (!result_role_assignment.Succeeded)
                    return BadRequest("Error assigning role to user");

                
                return Ok("The user has been created succefully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPost("UpdateRoles/{userid}")]

        public async Task<IActionResult> UpdateRoles(string userid,[FromBody] RoleINDTO value)
        {

            if (string.IsNullOrWhiteSpace(userid))
                return BadRequest("User id is required.");

            var user = await _userManager.FindByIdAsync(userid);
            if (user == null)
                return NotFound("User not found.");

            if (string.IsNullOrWhiteSpace(value?.Name))
                return BadRequest("Role name is required.");

            var role = await _roleManager.RoleExistsAsync(value.Name);
           

            //Possible enumeration attack, since we are revealing if a user exists or not.
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
