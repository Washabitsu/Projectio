using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Projectio.Core.Dtos;
using Projectio.Core.Models;
using Projectio.Persistence;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using Projectio.Helpers;
using Projectio.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Projectio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IJWT _jwt { get; set; }
        private IMapperWrapper _mapperWrapper { get; set; }
        private ApplicationDbContext _context { get; set; }

        private readonly UserManager<ApplicationUser> _userManager;
        public UserController(ApplicationDbContext context, IJWT jwt, IMapperWrapper mapperWrapper, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _mapperWrapper = mapperWrapper;
            _jwt = jwt;
            _userManager = userManager;
        }


        [HttpGet]
        public async Task<IActionResult> Get()
        {
            Request.Headers.TryGetValue("Authorization", out var token);
            var username = await _jwt.GetUsernameFJTW(token);
            if (username == null)
                return NotFound("Username not found!");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);


            if (user == null)
                return NotFound("User not found!");

            var dto = _mapperWrapper.Map<ApplicationUser, UserOutDTO>(user);
            var role_id = (await _context.UserRoles.FirstOrDefaultAsync(ru => ru.UserId == user.Id)).RoleId;
            if (role_id != null)
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == role_id);
                if (role != null)
                {
                    dto.Role = role.Name;
                }
            }

            return Ok(dto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Post([FromBody] UserInDTO value)
        {

            var user = new ApplicationUser()
            {
                UserName = value.Username,
                Email = value.Email,
            };

           
            try
            {
                IdentityRole new_role = null;
                if (value.RoleId != null)
                {
                    new_role = await _context.Roles.Where(r => r.Id == value.RoleId).FirstAsync();
                }
                else if (value.Role != null)
                {
                    new_role = await _context.Roles.Where(r => r.NormalizedName == value.Role.ToUpper()).FirstAsync();
                }


                var result = await _userManager.CreateAsync(user, value.Password);
                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                if (new_role != null)
                    await _userManager.AddToRoleAsync(user, new_role.Name);
                

                return Ok("The user has been created succefully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPut]
        public async Task<IActionResult> Put([FromBody] UserInDTO value)
        {
            Request.Headers.TryGetValue("Authorization", out var token);
            ApplicationUser user;
            var username = await _jwt.GetRoleFJTW(token);
            var role = await _jwt.GetRoleFJTW(token);
            if (value.UserId != null)
            {
                if (role == "Admin")
                    user = (await _context.Users.FindAsync(value.UserId));
                else
                    return Unauthorized("You are not authorized!");
            }
            else
            {
                if (username == null)
                    return NotFound("Username not found!");
                user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
            }

            if (user == null)
                return NotFound("User not found!");

            user.UpdateUser(value);

            if (!string.IsNullOrEmpty(value.Password))
            {
                if (string.IsNullOrEmpty(value.CurrentPassword))
                    return BadRequest("Current password is required");

                var passwordResult = await _userManager.ChangePasswordAsync(user, value.CurrentPassword, value.Password);
            }

            if (value.RoleId != null)
            {
                var new_role = await _context.Roles.FirstAsync(r => r.Id == value.RoleId);
                var existing_roles = (from ur in _context.UserRoles join r in _context.Roles on ur.RoleId equals r.Id where ur.UserId == user.Id select r.Name).ToList();
                foreach (var existing_role in existing_roles)
                    await _userManager.RemoveFromRoleAsync(user, existing_role);
                await _userManager.AddToRoleAsync(user, new_role.Name);
            }
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return Ok("User has been updated!");
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                Request.Headers.TryGetValue("Authorization", out var token);
                ApplicationUser user;
                var username = await _jwt.GetRoleFJTW(token);
                var role = await _jwt.GetRoleFJTW(token);
                if (id != null)
                {
                    if (role == "Admin")
                        user = (await _context.Users.FindAsync(id));
                    else
                        return Unauthorized("You are not authorized!");
                }
                else
                {
                    if (username == null)
                        return NotFound("Username not found!");
                    user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
                }

                if (user == null)
                    return NotFound("User not found!");

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
