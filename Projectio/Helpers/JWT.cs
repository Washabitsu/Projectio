using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Projectio.Core.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Projectio.Helpers
{
    public class JWT : IJWT
    {
        /* IJWTConfiguration _jwtConfiguration { get; set; }*/

        public IJWTConfiguration _jwtConfiguration { get; set; }
        public JWT(IJWTConfiguration jWTConfiguration)
        {
            _jwtConfiguration = jWTConfiguration;
        }

        public async Task<JwtSecurityToken> GetJwtToken(string username, List<Claim> additionalClaims = null)
        {
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub,username),
            // this guarantees the token is unique
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            if (additionalClaims is object)
            {
                var claimList = new List<Claim>(claims);
                claimList.AddRange(additionalClaims);
                claims = claimList.ToArray();
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.SigningKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            return new JwtSecurityToken(
                issuer: _jwtConfiguration.Issuer,
                audience: _jwtConfiguration.Audience,
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(_jwtConfiguration.TokenTimeoutMinutes)),
                claims: claims,
                signingCredentials: creds
            );
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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.SigningKey))
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
