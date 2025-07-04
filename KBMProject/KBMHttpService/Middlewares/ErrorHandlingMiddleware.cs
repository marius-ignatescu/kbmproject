using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace KBMHttpService.Middlewares
{
    /// <summary>
    /// Middleware for handling errors
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context); // Call the next middleware
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "gRPC error: {Message}", ex.Status.Detail);
                await HandleGrpcExceptionAsync(context, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred.");
                await HandleUnexpectedExceptionAsync(context, ex);
            }
        }

        private static Task HandleGrpcExceptionAsync(HttpContext context, RpcException ex)
        {
            var statusCode = ex.StatusCode switch
            {
                StatusCode.NotFound => HttpStatusCode.NotFound,
                StatusCode.AlreadyExists => HttpStatusCode.Conflict,
                StatusCode.InvalidArgument => HttpStatusCode.BadRequest,
                _ => HttpStatusCode.BadGateway
            };

            var problem = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = ex.Status.Detail,
                Type = $"https://httpstatuses.com/{(int)statusCode}"
            };

            var result = JsonSerializer.Serialize(problem);
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)statusCode;

            return context.Response.WriteAsync(result);
        }

        private static Task HandleUnexpectedExceptionAsync(HttpContext context, Exception ex)
        {
            var statusCode = HttpStatusCode.InternalServerError;
            var problem = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = "An unexpected error occurred.",
                Type = $"https://httpstatuses.com/{(int)statusCode}",
                Detail = ex.Message
            };

            var result = JsonSerializer.Serialize(problem);
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)statusCode;

            return context.Response.WriteAsync(result);
        }
    }
}
