using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Projectio.Exceptions;
using Projectio.Security.Interfaces.JWT;
using Projectio.Security.Interfaces.KeyManagement;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Projectio.Helpers
{
    public class JWT : IJWT
    {

        public IJWTConfiguration _jwtConfiguration { get; set; }
        public IEncryptionProvider _encryptionProvider { get; set; }

        public JWT(IJWTConfiguration jWTConfiguration, IEncryptionProvider encryptionProvider)
        {
            _jwtConfiguration = jWTConfiguration;
            _encryptionProvider = encryptionProvider;
        }

        public async Task<string> SignJwtToken( string username, List<Claim> additionalClaims = null)
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

            
          
            var creds = await _encryptionProvider.GetCredentials();

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDesciption = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtConfiguration.TokenTimeoutMinutes),
                Issuer = _jwtConfiguration.Issuer,
                Audience = _jwtConfiguration.Audience,
                SigningCredentials = creds,
                
            };

           

            var token = tokenHandler.CreateToken(tokenDesciption);
            var jwt = tokenHandler.WriteToken(token);
            

            return jwt;
        }

        public async Task<ClaimsPrincipal?> ValidateToken(StringValues bearer_token)
        {
            string token;
            try
            {
                token = bearer_token[0].Split(' ')[1];
                if (token == null)
                    throw new TokenValidationException();

                SecurityToken validatedToken = null;
                ClaimsPrincipal principal = null;
                var tokenHandler = new JwtSecurityTokenHandler();
                try
                {
                    principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateLifetime = true,
                        ValidateIssuer = true,
                        ValidIssuer = _jwtConfiguration.Issuer,
                        ValidateAudience = true,
                        ValidAudience = _jwtConfiguration.Audience,
                        ValidateIssuerSigningKey = true,
                        RequireExpirationTime = true,
                        RequireSignedTokens = true,
                        IssuerSigningKey = await _encryptionProvider.GetPublicKey()
                    }, out validatedToken);
                }
                catch (Exception ex)
                {
                    principal = null;
                }
                if ( validatedToken is null)
                    return null;

                return principal;
            }
   
            catch (Exception ex)
            {
                throw new TokenValidationException();
            }
        }
        public async Task<string?> GetUsernameFromToken(StringValues bearer_token)
        {
            try
            {
                var jwtToken = await ValidateToken(bearer_token);
                if (jwtToken is null)
                    return null;

                var userId = jwtToken.Claims.First(x => x.Type == "sub").Value;

                return userId;
            }
            catch (Exception ex )
            {
               throw new TokenValidationException();
            }
        }

        public async Task<IEnumerable<string?>> GetRolesFromToken(StringValues bearer_token)
        {
            try
            {
                var jwtToken = await ValidateToken(bearer_token);
                var roles = jwtToken.Claims
                    .Where(x => x.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                    .Select(x => x.Value);
                return roles;
            }
            catch (Exception ex)
            {
                throw new TokenValidationException();
            }
        }
    }
}
