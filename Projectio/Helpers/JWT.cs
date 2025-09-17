using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Projectio.Core.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Projectio.Helpers
{
    public class JWT : IJWT
    {
        /* IJWTConfiguration _jwtConfiguration { get; set; }*/

        public IJWTConfiguration _jwtConfiguration { get; set; }
        public IJWTProvider _jWTProvider { get; set; }

        public JWT(IJWTConfiguration jWTConfiguration, IJWTProvider jWTProvider)
        {
            _jwtConfiguration = jWTConfiguration;
            _jWTProvider = jWTProvider;
        }

        public async Task<String> GetJwtToken( string username, List<Claim> additionalClaims = null)
        {
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub,username),
            // this guarantees the token is unique
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())};
            if (additionalClaims is object)
            {
                var claimList = new List<Claim>(claims);
                claimList.AddRange(additionalClaims);
                claims = claimList.ToArray();
            }

            
            var rsa = _jWTProvider.GetPrivateKey();
            var creds = new SigningCredentials(rsa, SecurityAlgorithms.RsaSha256);

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDesciption = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtConfiguration.TokenTimeoutMinutes),
                Issuer = _jwtConfiguration.Issuer,
                Audience = _jwtConfiguration.Audience,
                SigningCredentials = creds
            };

            var token = tokenHandler.CreateToken(tokenDesciption);
            var jwt = tokenHandler.WriteToken(token);

            return jwt;
        }

        public async Task<string?> GetUsernameFJTW(StringValues bearer_token)
        {
            string token;
            try
            {
                token = bearer_token[0].Split(' ')[1];
            }
            catch (Exception ex)
            {
                token = null;
            }

            if (token == null)
                return null;


            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _jwtConfiguration.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtConfiguration.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _jWTProvider.GetPublicKey()
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = jwtToken.Claims.First(x => x.Type == "sub").Value;
                return userId;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string?> GetRoleFJTW(StringValues bearer_token)
        {
            string token;
            try
            {
                token = bearer_token[0].Split(' ')[1];
            }
            catch (Exception ex)
            {
                token = null;
            }

            if (token == null)
                return null;


            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _jwtConfiguration.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtConfiguration.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.SigningKey))
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = jwtToken.Claims.First(x => x.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role").Value;
                return userId;
            }
            catch
            {
                return null;
            }
        }
    }
}
