using Microsoft.IdentityModel.Tokens;
using Projectio.Security.Interfaces.KeyManagement;

namespace Projectio.Security.KeyManagement
{
    public abstract class SignKeyProviderBase : IEncryptionProvider
    {
        protected byte[]? _privateKeyBytes;
        public SignKeyProviderBase() { }

        public abstract Task<SigningCredentials> GetCredentials();

        public abstract Task<JsonWebKey> GetJWKS();

        public abstract Task<SecurityKey> GetPublicKey();
    }
}
