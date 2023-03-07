using Projectio.Core.Interfaces;

namespace Projectio.Core.Models
{
    public class JWTConfiguration : IJWTConfiguration
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string SigningKey { get; set; }
        public int TokenTimeoutMinutes { get; set; }
    }
}
