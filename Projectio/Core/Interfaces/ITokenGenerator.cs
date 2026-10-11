using Projectio.Security.Interfaces.JWT;
using System.Security.Claims;

namespace Projectio.Core.Interfaces
{
    public interface ITokenGenerator
    {
        public abstract static string GenerateJwtToken(string username, IJWTConfiguration jwtConfig, List<Claim> additionalClaims = null);
        public abstract static string GenerateRandomToken(int length = 32);
    }
}
