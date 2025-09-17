using Microsoft.IdentityModel.Tokens;

namespace Projectio.Core.Interfaces
{
    //Interface for JWT Provider to get RSA keys.
    public interface IJWTProvider
    {
        RsaSecurityKey GetPrivateKey();
        RsaSecurityKey GetPublicKey();
    }
}
