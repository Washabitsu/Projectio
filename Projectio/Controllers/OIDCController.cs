using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Projectio.Core.Dtos;
using Projectio.Core.Models;
using Projectio.Helpers;
using Projectio.Persistence;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using Projectio.Security.Interfaces.JWT;
using Projectio.Security.Interfaces.OAuth;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;


namespace Projectio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OIDCController : ControllerBase
    {
        private readonly IJWTConfiguration _jwtConfiguration;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IGoogleSettings _googleSettings;
        private readonly IConfiguration _configuration;

        public OIDCController(IJWTConfiguration jwtConfiguration, ApplicationDbContext context, UserManager<ApplicationUser> userManager, IGoogleSettings googleSettings, IConfiguration configuration)
        {
            _jwtConfiguration = jwtConfiguration;
            _context = context;
            _userManager = userManager;
            _googleSettings = googleSettings;
            _configuration = configuration;
        }


        [AllowAnonymous]
        [HttpGet("google/authorize")]
        public IActionResult GoogleAuthorize()
        {

            var props = new AuthenticationProperties
            {
                RedirectUri = "https://localhost:7256/api/OIDC/google/complete",

            };
            return Challenge(props, "GoogleOpenIdConnect");
        }

        /// <summary>
        /// OAuth 2.0 callback endpoint - handles Google's redirect after user authentication
        /// Exchanges authorization code for ID token and creates/finds user
        /// </summary>
        [AllowAnonymous]
        [HttpGet("google/complete")]
        public async Task<IActionResult> GoogleCallback()
        {
            try
            {
                var result = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
                if (!result.Succeeded)
                    return BadRequest(new { error = "Google authentication failed" });

                var claims = result.Principal.Claims;
                var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                if (string.IsNullOrEmpty(email))
                    return BadRequest(new { error = "Email not found in Google token" });

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true
                    };
                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                        return BadRequest(new { error = "Failed to create user account" });
                }

                var roles = await _userManager.GetRolesAsync(user);
                var tokenClaims = new List<Claim>();
                foreach (var role in roles)
                    tokenClaims.Add(new Claim(ClaimTypes.Role, role));
                tokenClaims.Add(new Claim(ClaimTypes.Email, user.Email));

                var token = TokenGenerator.GenerateJwtToken(user.UserName, _jwtConfiguration, tokenClaims);

                

                // Get frontend URL from configuration
                var frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:3000";

                // Redirect to frontend with token in query parameter
                return Redirect($"{frontendUrl}/auth/callback?token={token}&username={user.UserName}");
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "An error occurred during Google authentication", details = ex.Message });
            }
        }
    }
}
