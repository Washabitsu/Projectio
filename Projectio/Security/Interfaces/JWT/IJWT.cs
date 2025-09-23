using Microsoft.Extensions.Primitives;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Projectio.Security.Interfaces.JWT
{
    public interface IJWT
    {
        Task<string> SignJwtToken(string username, List<Claim> additionalClaims = null);
        Task<string?> GetUsernameFromToken(StringValues bearer_token);
        Task<ClaimsPrincipal?> ValidateToken(StringValues bearer_token);
        Task<IEnumerable<string?>> GetRolesFromToken(StringValues bearer_token);
    }
}
