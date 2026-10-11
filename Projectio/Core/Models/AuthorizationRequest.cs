namespace Projectio.Core.Models
{
    /// <summary>
    /// A login started by a client. Kept in memory while the user is at Google
    /// </summary>
    public class AuthorizationRequest
    {
        public string ClientId { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string CodeChallenge { get; set; } = string.Empty;
    }
}
