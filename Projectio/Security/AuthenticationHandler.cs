using Azure.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Projectio.Core.Interfaces;
using Projectio.Core.Models;
using Projectio.Logs;
using Projectio.Exceptions;
using System.Diagnostics.Tracing;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.IdentityModel.Abstractions;


namespace Projectio.Security
{
    public class AuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IJWT _jwt;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AuthenticationHandler> _logger;
        private readonly ILogEntryFactory _logFactory;

        public AuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            ILogEntryFactory logEntryFactory,
            UrlEncoder encoder,
            IJWT jwt,
            UserManager<ApplicationUser> userManager)
            : base(options, logger, encoder)
        {
            _jwt = jwt;
            _userManager = userManager;
            _logFactory = logEntryFactory;
            _logger = logger.CreateLogger<AuthenticationHandler>();
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                if (!Request.Headers.TryGetValue("Authorization", out StringValues token) || token == StringValues.Empty)
                    return AuthenticateResult.NoResult();

                string username;
            
                username = await _jwt.GetUsernameFJTW(token);

                LogEntry logEntry;

                
                if (string.IsNullOrEmpty(username))
                {
                    logEntry = _logFactory.CreateTokenValidationLog(Context);
                    return AuthenticateResult.Fail("Invalid Token");
                }
                var user = await _userManager.FindByNameAsync(username);

                if (user == null)
                {
                    logEntry = _logFactory.CreateTokenMissMatchLog(Context);
                    return AuthenticateResult.Fail("Invalid Token");
                }


                if (await _userManager.IsLockedOutAsync(user))
                {
                    logEntry = _logFactory.CreateLockedOutLog(Context);
                    return AuthenticateResult.Fail("You have been locked out. Contact Admin!");
                }
                
                var roles = await _userManager.GetRolesAsync(user);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, username)
                };

                claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                Context.Items["CurrentUser"] = user;
                return AuthenticateResult.Success(ticket);
            }
            catch
            {
                return AuthenticateResult.Fail("Something went wrong");
            }
        }
    }
}

