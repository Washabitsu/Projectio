using Microsoft.AspNetCore.Authentication;

namespace Projectio.Security.Interfaces.OAuth
{
    public interface IOAuthProvider
    {
        public Task ConfigureAuthentication(AuthenticationBuilder builder);

    }
}
