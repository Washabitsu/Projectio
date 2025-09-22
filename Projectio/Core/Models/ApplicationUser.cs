using Microsoft.AspNetCore.Identity;
using Projectio.Core.Dtos;

namespace Projectio.Core.Models
{
    public class ApplicationUser : IdentityUser
    {
        
        public IEnumerable<RefreshToken> RefreshTokens { get; set; }


        public void UpdateUser(UserInDTO dato)
        {
          
            if (dato.PhoneNumber != null)
                this.PhoneNumber = dato.PhoneNumber;
            if (dato.TwoFactorEnabled != null)
                this.TwoFactorEnabled = (bool)dato.TwoFactorEnabled;
        }

    }
}
