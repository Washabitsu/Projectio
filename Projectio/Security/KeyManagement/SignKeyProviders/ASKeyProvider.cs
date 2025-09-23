using Microsoft.IdentityModel.Tokens;
using Projectio.Security.Interfaces;
using Projectio.Security.Interfaces.KeyManagement;
using Projectio.Security.Interfaces.Signing;
using System.Security.Cryptography;

namespace Projectio.Security.KeyManagement.SKProviders
{
    public class ASKeyProvider : SignKeyProviderBase
    {
        private readonly string _pk;
        private readonly ISigner _signer;
        public ASKeyProvider(ISigner signer,string privateKeyBase64)
        {
            _pk = privateKeyBase64;
            _signer = signer;
        }

        public override async Task<SigningCredentials> GetCredentials() =>  await _signer.SignAsync(_pk);

        public override async  Task<SecurityKey> GetPublicKey() => await _signer.RetrievePublicKey(_pk);     
        
        public override async Task<JsonWebKey> GetJWKS() => await _signer.GetJWK(_pk);

       
    }
}
