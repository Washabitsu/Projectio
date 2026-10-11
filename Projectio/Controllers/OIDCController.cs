using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Projectio.Core.Models;
using Projectio.Helpers;
using Projectio.Security.Tools;
using System.Security.Claims;


namespace Projectio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OIDCController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;
        private readonly ClientStore _clients;


        public OIDCController(UserManager<ApplicationUser> userManager, IMemoryCache cache, ClientStore clients)
        {
            _userManager = userManager;
            _cache = cache;
            _clients = clients;
        }


        /// <summary>
        /// Step 1 - A client (Portio BFF) sends the user here to log in.
        /// We check the client, remember the request and send the user to Google
        /// </summary>
        [AllowAnonymous]
        [HttpGet("authorize")]
        public IActionResult Authorize(
            [FromQuery(Name = "client_id")] string clientId,
            [FromQuery(Name = "redirect_uri")] string redirectUri,
            [FromQuery(Name = "response_type")] string responseType,
            [FromQuery(Name = "state")] string state,
            [FromQuery(Name = "code_challenge")] string codeChallenge,
            [FromQuery(Name = "code_challenge_method")] string codeChallengeMethod)
        {
            // Unknown client or redirect address: show an error, never redirect (open redirect protection)
            var client = _clients.Find(clientId);
            if (client == null || client.RedirectUri != redirectUri)
                return BadRequest(new { error = "invalid_client" });

            if (responseType != "code" || codeChallengeMethod != "S256")
                return BadRequest(new { error = "invalid_request" });

            var requestId = TokenGenerator.GenerateRandomToken();
            _cache.Set("auth_request:" + requestId, new AuthorizationRequest
            {
                ClientId = client.ClientId,
                RedirectUri = client.RedirectUri,
                State = state,
                CodeChallenge = codeChallenge
            }, TimeSpan.FromMinutes(10));

            // The request id travels with the Google login and comes back in the callback
            var props = new AuthenticationProperties
            {
                RedirectUri = "/api/OIDC/google/complete"
            };
            props.Items["request_id"] = requestId;

            return Challenge(props, "GoogleOpenIdConnect");
        }

        /// <summary>
        /// Step 2 - Google sent the user back. We find or create the user, create a one-time code
        /// and send the user back to the client with it
        /// </summary>
        [AllowAnonymous]
        [HttpGet("google/complete")]
        public async Task<IActionResult> GoogleCallback()
        {
            try
            {
                var result = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
                if (!result.Succeeded || result.Principal == null || result.Properties == null)
                    return BadRequest(new { error = "Google authentication failed" });

                // The Google cookie is only needed for this request
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

                result.Properties.Items.TryGetValue("request_id", out var requestId);
                var request = _cache.Get<AuthorizationRequest>("auth_request:" + requestId);
                if (request == null)
                    return BadRequest(new { error = "Login request expired. Please try again." });
                _cache.Remove("auth_request:" + requestId);

                var email = result.Principal.FindFirstValue(ClaimTypes.Email);
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

                // One-time code: valid for 1 minute, useless without the PKCE verifier
                var code = TokenGenerator.GenerateRandomToken();
                _cache.Set("auth_code:" + code, new AuthorizationCode
                {
                    UserId = user.Id,
                    ClientId = request.ClientId,
                    RedirectUri = request.RedirectUri,
                    CodeChallenge = request.CodeChallenge
                }, TimeSpan.FromMinutes(1));

                // Only the code and the client's own state go in the URL - never a token or user data
                return Redirect($"{request.RedirectUri}?code={code}&state={Uri.EscapeDataString(request.State)}");
            }
            catch (Exception)
            {
                return BadRequest(new { error = "An error occurred during Google authentication" });
            }
        }
    }
}
