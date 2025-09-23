using Microsoft.IdentityModel.Tokens;

namespace Projectio.Security.Interfaces.KeyManagement
{
    //Interface for JWT Provider to get RSA keys.
    public interface IEncryptionProvider
    {
        Task<SigningCredentials> GetCredentials();
        Task<SecurityKey> GetPublicKey();
        Task<JsonWebKey> GetJWKS();


    }
}
