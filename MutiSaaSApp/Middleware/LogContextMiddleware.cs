using Serilog.Context;
using Serilog;

namespace MutiSaaSApp.Middleware
{
    /// <summary>
    /// Middleware for enriching logs with request context information.
    /// Adds RequestId, UserId, OrgId, and other relevant context to all log entries.
    /// </summary>
    public class LogContextMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LogContextMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the LogContextMiddleware class.
        /// </summary>
        public LogContextMiddleware(RequestDelegate next, ILogger<LogContextMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the middleware to enrich log context.
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            // Generate or retrieve RequestId
            var requestId = context.TraceIdentifier;
            using (LogContext.PushProperty("RequestId", requestId))
            {
                // Extract UserId from JWT claims if available
                var userId = context.User?.FindFirst("sub")?.Value ?? context.User?.FindFirst("userId")?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    LogContext.PushProperty("UserId", userId);
                }

                // Extract OrgId from claims if available
                var orgId = context.User?.FindFirst("orgId")?.Value;
                if (!string.IsNullOrEmpty(orgId))
                {
                    LogContext.PushProperty("OrgId", orgId);
                }

                // Add request metadata
                using (LogContext.PushProperty("RequestPath", context.Request.Path))
                using (LogContext.PushProperty("RequestMethod", context.Request.Method))
                using (LogContext.PushProperty("RemoteIP", context.Connection.RemoteIpAddress?.ToString()))
                {
                    // Log request
                    _logger.LogInformation(
                        "HTTP {RequestMethod} {RequestPath} started",
                        context.Request.Method,
                        context.Request.Path);

                    try
                    {
                        await _next(context);

                        // Log response
                        _logger.LogInformation(
                            "HTTP {RequestMethod} {RequestPath} completed with status {StatusCode}",
                            context.Request.Method,
                            context.Request.Path,
                            context.Response.StatusCode);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "HTTP {RequestMethod} {RequestPath} failed with exception",
                            context.Request.Method,
                            context.Request.Path);
                        throw;
                    }
                }
            }
        }
    }
}
