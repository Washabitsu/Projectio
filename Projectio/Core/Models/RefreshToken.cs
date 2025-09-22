using System.ComponentModel.DataAnnotations;

namespace Projectio.Core.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }


        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }


        [Required]
        public byte[] RefreshTokenHash { get; set; }

        public byte[]? ReplacedByTokenHash { get; set; }

        [Required]
        public string CreatedByIp { get; set; }
        public string? CreatedByUserAgent { get; set; }

        public string? RevokedByIp { get; set; }


        [Required]
        public DateTimeOffset Created { get; set; }
        [Required]
        public DateTimeOffset Expires { get; set; }
        public DateTimeOffset? Revoked { get; set; }


        // Convenience
        public bool IsExpired => DateTimeOffset.UtcNow >= Expires;
        public bool IsRevoked => Revoked.HasValue;
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}
