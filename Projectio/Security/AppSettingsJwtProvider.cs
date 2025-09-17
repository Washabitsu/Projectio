using Microsoft.IdentityModel.Tokens;
using Projectio.Core.Interfaces;
using System.Security.Cryptography;

namespace Projectio.Security
{
    public class AppSettingsJwtProvider : IJWTProvider
    {
        private readonly string _pk;
        public AppSettingsJwtProvider(string privateKeyBase64)
        {
            _pk = privateKeyBase64;
        }

        public RsaSecurityKey GetPrivateKey()
        {
            var rsa = RSA.Create();
            var keyBytes = Convert.FromBase64String(_pk);
            rsa.ImportPkcs8PrivateKey(keyBytes, out _);
            return new RsaSecurityKey(rsa);
        }

        public RsaSecurityKey GetPublicKey()
        {
            var rsa = RSA.Create();
            var private_bytes = Convert.FromBase64String(_pk);
            rsa.ImportPkcs8PrivateKey(private_bytes, out _);
            byte[] publicKeyBytes = rsa.ExportSubjectPublicKeyInfo();
            rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
            return new RsaSecurityKey(rsa);
        }
    }
}
