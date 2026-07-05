using Microsoft.IdentityModel.Tokens;
using Projectio.Core.Models;
using Projectio.Security.Interfaces.JWT;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Claims;

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

            var signingCredentials = new SigningCredentials(
                new RsaSecurityKey(rsa),
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
    }
}
