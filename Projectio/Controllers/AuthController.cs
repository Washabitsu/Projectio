using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Projectio.Core.Dtos;
using Projectio.Core.Models;
using Projectio.Helpers;
using Projectio.Persistence;
using Projectio.Security.Interfaces.JWT;
using Projectio.Security.Tools;
using System.Security.Claims;


namespace Projectio.Controllers
{
    /// <summary>
    /// Token endpoints. Called server to server by a client (Portio BFF), never by the browser
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IJWTConfiguration _jwtConfiguration;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;
        private readonly ClientStore _clients;

        public AuthController(IJWTConfiguration jwtConfiguration, ApplicationDbContext context, UserManager<ApplicationUser> userManager, IMemoryCache cache, ClientStore clients)
        {
            _jwtConfiguration = jwtConfiguration;
            _context = context;
            _userManager = userManager;
            _cache = cache;
            _clients = clients;
        }


        /// <summary>
        /// Step 3 - The client swaps the one-time code for an access token and a refresh token
        /// </summary>
        [AllowAnonymous]
        [HttpPost("exchange")]
        public async Task<ActionResult> Exchange(OIDCDto dto)
        {
            var client = _clients.Validate(dto.ClientId, dto.ClientSecret);
            if (client == null)
                return Unauthorized(new { error = "invalid_client" });

            // Remove right away: a code can only be used once
            var code = _cache.Get<AuthorizationCode>("auth_code:" + dto.ExchangeCode);
            _cache.Remove("auth_code:" + dto.ExchangeCode);

            if (code == null || code.ClientId != client.ClientId || code.RedirectUri != dto.RedirectUri)
                return BadRequest(new { error = "invalid_grant" });

            if (!PkceToolset.Verify(dto.CodeVerifier, code.CodeChallenge))
                return BadRequest(new { error = "invalid_grant" });

            var user = await _userManager.FindByIdAsync(code.UserId);
            if (user == null)
                return BadRequest(new { error = "invalid_grant" });

            var tokens = await CreateTokens(user);
            await _context.SaveChangesAsync();

            return Ok(tokens);
        }


        /// <summary>
        /// The client sends a refresh token and gets new tokens.
        /// The old refresh token is revoked (rotation), so each one works only once
        /// </summary>
        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<ActionResult> Refresh(RefreshDto dto)
        {
            var client = _clients.Validate(dto.ClientId, dto.ClientSecret);
            if (client == null)
                return Unauthorized(new { error = "invalid_client" });

            var stored = await FindRefreshToken(dto.RefreshToken);
            if (stored == null || !stored.IsActive)
                return Unauthorized(new { error = "invalid_grant" });

            var tokens = await CreateTokens(stored.User);

            stored.Revoked = DateTimeOffset.UtcNow;
            stored.RevokedByIp = GetIp();
            stored.ReplacedByTokenHash = TokenGenerator.HashToken(tokens.RefreshToken);
            await _context.SaveChangesAsync();

            return Ok(tokens);
        }


        /// <summary>
        /// Logout - the refresh token can't be used anymore
        /// </summary>
        [AllowAnonymous]
        [HttpPost("revoke")]
        public async Task<ActionResult> Revoke(RefreshDto dto)
        {
            var client = _clients.Validate(dto.ClientId, dto.ClientSecret);
            if (client == null)
                return Unauthorized(new { error = "invalid_client" });

            var stored = await FindRefreshToken(dto.RefreshToken);
            if (stored != null && stored.IsActive)
            {
                stored.Revoked = DateTimeOffset.UtcNow;
                stored.RevokedByIp = GetIp();
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }


        private async Task<TokenDto> CreateTokens(ApplicationUser user)
        {
            var claims = new List<Claim>();
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
                claims.Add(new Claim("role", role));
            if (user.Email != null)
                claims.Add(new Claim("email", user.Email));

            var accessToken = TokenGenerator.GenerateJwtToken(user.UserName!, _jwtConfiguration, claims);

            // The client gets the token, the database only gets its hash.
            // Saved by the caller with SaveChangesAsync
            var refreshToken = TokenGenerator.GenerateRandomToken();
            _context.Set<RefreshToken>().Add(new RefreshToken
            {
                UserId = user.Id,
                RefreshTokenHash = TokenGenerator.HashToken(refreshToken),
                CreatedByIp = GetIp(),
                CreatedByUserAgent = Request.Headers.UserAgent.ToString(),
                Created = DateTimeOffset.UtcNow,
                Expires = DateTimeOffset.UtcNow.AddMinutes(_jwtConfiguration.RefreshTokenTimeoutMinutes)
            });

            return new TokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = _jwtConfiguration.TokenTimeoutMinutes * 60
            };
        }

        private async Task<RefreshToken?> FindRefreshToken(string refreshToken)
        {
            var hash = TokenGenerator.HashToken(refreshToken);
            return await _context.Set<RefreshToken>()
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.RefreshTokenHash == hash);
        }

        private string GetIp()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
