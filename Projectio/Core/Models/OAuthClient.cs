namespace Projectio.Core.Models
{
    /// <summary>
    /// An application that is allowed to log users in through Projectio (for example the Portio BFF)
    /// </summary>
    public class OAuthClient
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
    }
}
