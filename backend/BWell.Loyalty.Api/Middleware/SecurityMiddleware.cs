using System.Net;
using System.Text.Json;
using BWell.Loyalty.Application.Common;

namespace BWell.Loyalty.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            await WriteErrorAsync(context, ex);
        }
    }

    private async Task WriteErrorAsync(HttpContext context, Exception ex)
    {
        var (status, message) = ex switch
        {
            ArgumentException => (HttpStatusCode.BadRequest, ex.Message),
            InvalidOperationException => (HttpStatusCode.BadRequest, ex.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized."),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        if (status == HttpStatusCode.InternalServerError)
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);
        else
            _logger.LogWarning(ex, "Request rejected on {Method} {Path}", context.Request.Method, context.Request.Path);

        if (context.Response.HasStarted)
            throw ex;

        context.Response.Clear();
        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json";

        var payload = ApiResponse<object>.Fail(message, status == HttpStatusCode.InternalServerError
            ? null
            : new[] { ex.Message });

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        headers["X-XSS-Protection"] = "0";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        headers["Cache-Control"] = "no-store";

        if (context.Request.IsHttps)
            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

        await _next(context);
    }
}
