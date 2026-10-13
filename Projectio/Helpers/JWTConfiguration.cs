using Projectio.Security.Interfaces.JWT;
using System.ComponentModel.DataAnnotations;

public class JWTConfiguration : IJWTConfiguration
{
    [Required]
    public string Issuer { get; set; }
    [Required]
    public IEnumerable<string> Audience { get; set; }
    [Required]
    public string SigningKey { get; set; }
    [Required]
    public int TokenTimeoutMinutes { get; set; }
    [Required]
    public int RefreshTokenTimeoutMinutes { get; set; }
}