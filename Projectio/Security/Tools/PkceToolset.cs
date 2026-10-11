using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

namespace Projectio.Security.Tools
{
    public static class PkceToolset
    {
        /// <summary>
        /// PKCE (S256): the client proves it started the login by sending the verifier
        /// whose SHA256 hash is the challenge we received in /authorize
        /// </summary>
        public static bool Verify(string codeVerifier, string codeChallenge)
        {
            var hash = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
            var computed = Base64Url.EncodeToString(hash);

            return CryptographicOperations.FixedTimeEquals(
                Encoding.ASCII.GetBytes(computed),
                Encoding.ASCII.GetBytes(codeChallenge));
        }
    }
}
