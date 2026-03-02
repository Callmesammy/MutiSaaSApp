using System.Net;
using System.Text.Json;
using Domain.Exceptions;
using MutiSaaSApp.Common;

namespace MutiSaaSApp.Middleware
{
    /// <summary>
    /// Global exception handling middleware.
    /// Catches all unhandled exceptions and returns consistent error responses.
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the GlobalExceptionMiddleware class.
        /// </summary>
        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the middleware to handle exceptions in the HTTP pipeline.
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Handles the exception and returns an appropriate HTTP response.
        /// Maps domain exceptions to HTTP status codes.
        /// </summary>
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ApiResponse<object>("An error occurred.");
            var statusCode = HttpStatusCode.InternalServerError;

            switch (exception)
            {
                case NotFoundException notFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    response = new ApiResponse<object>(notFoundException.Message);
                    break;

                case ConflictException conflictException:
                    statusCode = HttpStatusCode.Conflict;
                    response = new ApiResponse<object>(conflictException.Message);
                    break;

                case UnauthorizedException unauthorizedException:
                    statusCode = HttpStatusCode.Unauthorized;
                    response = new ApiResponse<object>(unauthorizedException.Message);
                    break;

                case ValidationException validationException:
                    statusCode = HttpStatusCode.UnprocessableEntity; // 422
                    response = new ApiResponse<object>("Validation failed.", validationException.Errors);
                    break;

                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    response = new ApiResponse<object>("An unexpected error occurred.");
                    break;
            }

            context.Response.StatusCode = (int)statusCode;
            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
