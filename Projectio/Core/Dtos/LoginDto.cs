using System.ComponentModel.DataAnnotations;

namespace Projectio.Core.Dtos
{
    public class LoginDto
    {
        [Required]
        public string ExchangeCode { get; set; } = string.Empty;
        [Required]
        public string CodeVerifier { get; set; } = string.Empty;
        [Required]
        public string RedirectUri { get; set; } = string.Empty;
        [Required]
        public string ClientId { get; set; } = string.Empty;
        [Required]
        public string ClientSecret { get; set; } = string.Empty;
    }
}
