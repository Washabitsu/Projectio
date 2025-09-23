namespace Projectio.Security.Interfaces.JWT
{
    public interface IJWTConfiguration
    {
        string Issuer { get; set; }
        string Audience { get; set; }    
        string SigningKey { get; set; }
        int TokenTimeoutMinutes { get; set; }
    }
}
