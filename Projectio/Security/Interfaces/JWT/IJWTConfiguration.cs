namespace Projectio.Security.Interfaces.JWT
{
    public interface IJWTConfiguration
    {
        string Issuer { get; }
        IEnumerable<string> Audience { get; }
        string SigningKey { get; }
        int TokenTimeoutMinutes { get; }
    }
}
