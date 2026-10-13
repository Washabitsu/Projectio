namespace Projectio.Security.Interfaces.OAuth
{
    public interface IOAuthSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string[] Scope { get;set; }

        public string[] RedirectUris { get; set; }

        public string AuthUri { get; set; }
        public string TokenUri { get; set; }
    }
}
