using Projectio.Core.Models;
using System.Security.Cryptography;
using System.Text;

namespace Projectio.Security.Tools
{
    /// <summary>
    /// The list of clients (from the Clients section of the configuration) that may use Projectio
    /// </summary>
    public class ClientStore
    {
        private readonly List<OAuthClient> _clients;

        public ClientStore(List<OAuthClient> clients)
        {
            _clients = clients;
        }

        public OAuthClient? Find(string? clientId)
        {
            return _clients.FirstOrDefault(c => c.ClientId == clientId);
        }

        public OAuthClient? Validate(string? clientId, string? clientSecret)
        {
            var client = Find(clientId);
            if (client == null || clientSecret == null)
                return null;

            // Compare in constant time so the secret can't be guessed from response times
            var expected = Encoding.UTF8.GetBytes(client.ClientSecret);
            var actual = Encoding.UTF8.GetBytes(clientSecret);
            return CryptographicOperations.FixedTimeEquals(expected, actual) ? client : null;
        }
    }
}
