using System.ComponentModel.DataAnnotations;

namespace Projectio.Core.Dtos
{
    public class TokenDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; } = "Bearer";
    }

    public class RefreshDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
        [Required]
        public string ClientId { get; set; } = string.Empty;
        [Required]
        public string ClientSecret { get; set; } = string.Empty;
    }
}
