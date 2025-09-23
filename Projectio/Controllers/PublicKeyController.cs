using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Projectio.Core.Interfaces;
using Projectio.Security.Interfaces.KeyManagement;

namespace Projectio.Controllers
{

    [ApiController]


    [Route(".well-known/jwks.json")]
    [Route("api/publickey")]
    public class PublicKeyController : ControllerBase
    {
        private readonly IEncryptionProvider _encryptionProvider;

        public PublicKeyController(IEncryptionProvider encryptionProvider)
        {
            _encryptionProvider = encryptionProvider;
        }

        [HttpGet]
        public async Task<IActionResult> GetJwks()
        {
            // Build JWKS response
            var jwk = await _encryptionProvider.GetJWKS();

            var jwks = new { keys = new[] { jwk } };

            return new JsonResult(jwks);
        }
    }

}