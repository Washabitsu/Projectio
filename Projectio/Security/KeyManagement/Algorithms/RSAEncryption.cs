using Microsoft.IdentityModel.Tokens;
using Projectio.Security.Interfaces.KeyManagement;
using Projectio.Security.Interfaces.Signing;
using System.Security.Cryptography;


namespace Projectio.Security.KeyManagement.Algorithms
{
    public class RSAEncryption : ISigner
    {
        public async Task<JsonWebKey> GetJWK(string privateKey)
        {
            var rsaKey = (RsaSecurityKey)await RetrievePublicKey(privateKey);
            var rsa = rsaKey.Rsa!;
            var parameters = rsa.ExportParameters(false);

            var jwk = new JsonWebKey
            {
                Kty = JsonWebAlgorithmsKeyTypes.RSA,                   // "RSA"
                N = Base64UrlEncoder.Encode(parameters.Modulus),     // modulus
                E = Base64UrlEncoder.Encode(parameters.Exponent),    // exponent
                Alg = SecurityAlgorithms.RsaSha256,                    // "RS256"
                Use = JsonWebKeyUseNames.Sig,                          // "sig"

            };

            jwk.Kid = Base64UrlEncoder.Encode(jwk.ComputeJwkThumbprint());
            return jwk;
        }
        

        public async Task<SecurityKey> RetrievePublicKey(string privateKey)
        {
            var rsa = RSA.Create();
            var private_bytes = Convert.FromBase64String(privateKey);
            rsa.ImportPkcs8PrivateKey(private_bytes, out _);
            byte[] publicKeyBytes = rsa.ExportSubjectPublicKeyInfo();
            rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
            return new RsaSecurityKey(rsa);
        }

        public async Task<SigningCredentials> SignAsync(string privateKey)
        {
            var rsa = RSA.Create();
            var keyBytes = Convert.FromBase64String(privateKey);
            rsa.ImportPkcs8PrivateKey(keyBytes, out _);
            return new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);
        }


    }
}
