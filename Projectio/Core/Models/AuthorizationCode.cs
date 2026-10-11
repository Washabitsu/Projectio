namespace Projectio.Core.Models
{
    /// <summary>
    /// The one-time code the client swaps for tokens. Lives for one minute and can be used once
    /// </summary>
    public class AuthorizationCode
    {
        public string UserId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
        public string CodeChallenge { get; set; } = string.Empty;
    }
}
