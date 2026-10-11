using Microsoft.IdentityModel.Tokens;
using System.Buffers.Text;
using System.Security.Cryptography;

namespace Projectio.Security.Tools
{
    public static class KeyToolset
    {
        public static RsaSecurityKey GetPublicKey(string privateKeyBase64)
        {
            using var rsa = RSA.Create();
            var privateKeyBytes = Convert.FromBase64String(privateKeyBase64);
            rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);

            // Export only the public part as plain parameters.
            // Returning the RSA object itself would hand out an object that is disposed when this method ends
            return new RsaSecurityKey(rsa.ExportParameters(includePrivateParameters: false))
            {
                KeyId = CreateKeyId(rsa)
            };
        }

        /// <summary>
        /// The "kid" of a key: a hash of its public part. The same key always gets the same id
        /// and a new key gets a new one, so services can tell which key signed a token (key rotation)
        /// </summary>
        public static string CreateKeyId(RSA rsa)
        {
            var hash = SHA256.HashData(rsa.ExportSubjectPublicKeyInfo());
            return Base64Url.EncodeToString(hash);
        }
    }
}
