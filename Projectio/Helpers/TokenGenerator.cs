using Microsoft.IdentityModel.Tokens;
using Projectio.Core.Models;
using Projectio.Security.Interfaces.JWT;
using Projectio.Security.Tools;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;

namespace Projectio.Helpers
{
    public static class TokenGenerator
    {
        public static string GenerateJwtToken(
            string username,
            IJWTConfiguration jwtConfig,
            List<Claim> additionalClaims = null)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (additionalClaims != null)
            {
                claims.AddRange(additionalClaims);
            }

            // Parse the private key from base64
            var rsa = RSA.Create();
            var privateKeyBytes = Convert.FromBase64String(jwtConfig.SigningKey!);
            rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);

            // The key id goes into the token header ("kid"), so a service can pick the matching key from /.well-known/jwks.json
            var signingCredentials = new SigningCredentials(
                new RsaSecurityKey(rsa) { KeyId = KeyToolset.CreateKeyId(rsa) },
                SecurityAlgorithms.RsaSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(jwtConfig.TokenTimeoutMinutes),
                Issuer = jwtConfig.Issuer,
                Audience = jwtConfig.Audience?.FirstOrDefault(),
                SigningCredentials = signingCredentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        public static string GenerateRandomToken(int length = 32)
        {
            // Hex instead of Base64: Base64 contains + / = which break when used in a URL
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(length));
        }

        /// <summary>
        /// Refresh tokens are stored only as a hash, so a database leak doesn't leak usable tokens
        /// </summary>
        public static byte[] HashToken(string token)
        {
            return SHA256.HashData(Encoding.UTF8.GetBytes(token));
        }
    }
}
 
