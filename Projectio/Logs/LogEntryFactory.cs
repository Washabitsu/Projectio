using Microsoft.IdentityModel.Abstractions;
using Projectio.Core.Interfaces;

namespace Projectio.Logs
{
    public class LogEntryFactory : ILogEntryFactory
    {
        public LogEntry Create(Exception exception)
        {
            throw new NotImplementedException();
        }

        public LogEntry CreateForbiddenLog(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public LogEntry CreateLockedOutLog(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public LogEntry CreateTokenMissMatchLog(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public LogEntry CreateTokenValidationLog(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public LogEntry CreateUnauthorizedLog(HttpContext context)
        {
            throw new NotImplementedException();
        }
    }
}
