using Microsoft.AspNetCore.Identity;
using Projectio.Core.Dtos;

namespace Projectio.Core.Models
{
    public class ApplicationUser : IdentityUser
    {

        public void UpdateUser(UserInDTO dato)
        {
            if (dato.Password != null)
                this.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dato.Password);
            if (dato.PhoneNumber != null)
                this.PhoneNumber = dato.PhoneNumber;
            if (dato.TwoFactorEnabled != null)
                this.TwoFactorEnabled = (bool)dato.TwoFactorEnabled;
        }

    }
}
