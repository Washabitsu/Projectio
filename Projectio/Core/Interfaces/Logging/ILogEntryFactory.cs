using Microsoft.IdentityModel.Abstractions;

namespace Projectio.Core.Interfaces.Logging
{
    public interface ILogEntryFactory
    {
        LogEntry Create(Exception exception);
        LogEntry CreateForbiddenLog(HttpContext context);
        LogEntry CreateUnauthorizedLog(HttpContext context);
        LogEntry CreateLockedOutLog(HttpContext context);
        LogEntry CreateTokenMissMatchLog(HttpContext context);
        LogEntry CreateTokenValidationLog(HttpContext context);

    }
}
