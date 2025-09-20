using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Server.IIS;
using Projectio.Exceptions;
using System.Net;

namespace Projectio.MiddleWare
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (LockedOutException exception)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Locked;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\": \"Your account is locked. Please contact support or try again later.\"}");
            }
            catch (UnauthorizedAccess exception)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\": \"Unauthorized access. Please check your credentials.\"}");
            }
            catch (ForbiddenException exception)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\": \"Forbidden. You do not have permission to access this resource.\"}");
            }
            catch (TokenMissMatchException exception)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\": \"Unauthorized access. Please check your credentials.\"}");
            }

            catch( TokenValidationException)
            {

                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\": \"Token validation faile. Please provide a valid token.\"}");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "An unhandled exception occurred.");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\": \"An unexpected error occurred. Please try again later.\"}");
            }
        }
    }
}
