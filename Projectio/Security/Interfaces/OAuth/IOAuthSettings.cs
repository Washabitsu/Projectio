namespace Projectio.Security.Interfaces.OAuth
{
    public interface IOAuthSettings
    {
        public string ClientId { get; set; }
        public string ProjectId { get; set; }
        public string AuthUri { get; set; }
        public string TokenUri { get; set; }
        public string AuthProviderX509CertUrl { get; set; }
        public string ClientSecret { get; set; }
        public string[] RedirectUris { get; set; }
    }
}
