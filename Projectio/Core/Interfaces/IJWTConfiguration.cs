namespace Projectio.Core.Interfaces
{
    public interface IJWTConfiguration
    {
        string Issuer { get; set; }
        string Audience { get; set; }    
        string SigningKey { get; set; }
        int TokenTimeoutMinutes { get; set; }
    }
}
