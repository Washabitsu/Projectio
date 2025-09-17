using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Projectio.Core.Interfaces;

namespace Projectio.Controllers
{

    [ApiController]


    [Route(".well-known/jwks.json")]
    [Route("api/publickey")]
    public class PublicKeyController : ControllerBase
    {
        private readonly IJWTProvider _jwtProvider;

        public PublicKeyController(IJWTProvider jwtProvider)
        {
            _jwtProvider = jwtProvider;
        }

        [HttpGet]
        public IActionResult GetJwks()
        {
            var rsaKey = _jwtProvider.GetPublicKey().Rsa;
            var parameters = rsaKey.ExportParameters(false);

            // Build JWKS response
            var jwk = new
            {
                kty = "RSA",
                n = Base64UrlEncoder.Encode(parameters.Modulus),
                e = Base64UrlEncoder.Encode(parameters.Exponent),
                alg = "RS256",
                use = "sig"
            };

            var jwks = new { keys = new[] { jwk } };

            return new JsonResult(jwks);
        }
    }

}