using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projectio.Security.Interfaces.JWT;
using Projectio.Security.Tools;
using System.Buffers.Text;

namespace Projectio.Controllers
{
    [ApiController]
    public class WellKnownController : ControllerBase
    {
        private readonly IJWTConfiguration _jwtConfiguration;

        public WellKnownController(IJWTConfiguration jwtConfiguration)
        {
            _jwtConfiguration = jwtConfiguration;
        }


        /// <summary>
        /// The public key(s) that sign our access tokens (JWKS).
        /// Other services (OrchestrML) fetch this to check a token without sharing a secret with us
        /// </summary>
        [AllowAnonymous]
        [HttpGet("/.well-known/jwks.json")]
        public IActionResult Jwks()
        {
            var key = KeyToolset.GetPublicKey(_jwtConfiguration.SigningKey);
            var parameters = key.Parameters;

            var jwk = new
            {
                kty = "RSA",
                use = "sig",
                alg = "RS256",
                kid = key.KeyId,
                n = Base64Url.EncodeToString(parameters.Modulus!),
                e = Base64Url.EncodeToString(parameters.Exponent!)
            };

            return Ok(new { keys = new[] { jwk } });
        }
    }
}
