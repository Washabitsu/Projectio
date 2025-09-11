using Microsoft.AspNetCore.Identity;
using Projectio.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Projectio.Core.Dtos
{
    public class UserDto
    {
        public string? UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        [StringLength(14)]
        public string? PhoneNumber { get; set; }
        public bool? TwoFactorEnabled { get; set; }
    }

    public class UserOutDTO : UserDto
    {
        public string? Role { get; set; }
    }

    public class UserInDTO : UserDto
    {
        public string? Password { get; set; }
        public string? CurrentPassword { get; set; }
        public string? RoleId { get; set; }
        public string? Role { get; set; }
    }

}
