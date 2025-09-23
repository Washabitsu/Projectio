using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;

namespace Projectio.Security.Interfaces.Signing
{
    public interface ISigner
    {
       
        public Task<SigningCredentials> SignAsync(string privateKey);
        public Task<SecurityKey> RetrievePublicKey(string privateKey);
        public Task<JsonWebKey> GetJWK(string privateKey);
    }
}
