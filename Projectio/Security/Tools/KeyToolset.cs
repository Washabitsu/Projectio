using Microsoft.IdentityModel.Tokens;

namespace Projectio.Security.Tools
{
    public static class KeyToolset
    {
        public static SecurityKey GetPublicKey(string privateKeyBase64)
        {
            using var rsa = System.Security.Cryptography.RSA.Create();
            var privateKeyBytes = Convert.FromBase64String(privateKeyBase64);
            rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);
            var publicKeyBytes = rsa.ExportSubjectPublicKeyInfo();

            using var publicRsa = System.Security.Cryptography.RSA.Create();
            publicRsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
            return new Microsoft.IdentityModel.Tokens.RsaSecurityKey(publicRsa);    
        }
    }
}
