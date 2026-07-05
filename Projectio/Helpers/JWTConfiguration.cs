using Projectio.Security.Interfaces.JWT;

public class JWTConfiguration : IJWTConfiguration
{
    public string Issuer { get; set; }
    public IEnumerable<string> Audience { get; set; }
    public string SigningKey { get; set; }
    public int TokenTimeoutMinutes { get; set; }
}