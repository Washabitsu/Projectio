using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Authentication;
using Projectio.Security.Interfaces.OAuth;

namespace Projectio.Security.Authorization.OAuthProvider
{
    public class GoogleProvider : IOAuthProvider
    {
        public IGoogleSettings Settings { get;  }

        public GoogleProvider(IGoogleSettings settings)
        {
            Settings = settings;
        }

        public Task ConfigureAuthentication(AuthenticationBuilder builder)
        {
            throw new NotImplementedException();
        }
    }
}
