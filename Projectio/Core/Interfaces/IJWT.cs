using Microsoft.Extensions.Primitives;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Projectio.Core.Interfaces
{
    public interface IJWT
    {
        Task<JwtSecurityToken> GetJwtToken(string username, List<Claim> additionalClaims = null);
        Task<string?> GetUsernameFJTW(StringValues bearer_token);
        Task<string?> GetRoleFJTW(StringValues bearer_token);
    }
}
