namespace Projectio.Security.Interfaces.OAuth.OAuthSettngs.Google
{
    public interface IGoogleSettings : IOAuthSettings
    {
        public string ProjectId { get; set; }
        public string AuthProviderX509CertUrl { get; set; }
    }
}
