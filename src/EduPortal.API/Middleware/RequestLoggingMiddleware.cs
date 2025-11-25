using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace EduPortal.API.Middleware;

/// <summary>
/// Logs all HTTP requests and responses for monitoring and debugging purposes.
/// Automatically redacts sensitive information like passwords and tokens.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    // Patterns to identify sensitive data that should be redacted
    private static readonly string[] SensitiveFields =
    {
        "password", "token", "secret", "apikey", "api_key", "authorization",
        "credit_card", "creditcard", "cvv", "ssn", "social_security",
        "access_token", "refresh_token", "bearer", "jwt", "cookie"
    };

    private static readonly Regex SensitiveDataRegex = new(
        $@"(""{string.Join("|", SensitiveFields)}""\s*:\s*)""[^""]*""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString("N")[..8];

        // Get client IP address
        var clientIp = GetClientIpAddress(context);

        // Log request
        await LogRequest(context, requestId, clientIp);

        // Capture the original response body stream
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            // Log response
            await LogResponse(context, requestId, clientIp, stopwatch.ElapsedMilliseconds);

            // Reset position before copying back to original stream
            responseBody.Seek(0, SeekOrigin.Begin);

            // Copy the response body back to the original stream
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private async Task LogRequest(HttpContext context, string requestId, string clientIp)
    {
        var request = context.Request;

        // Read request body if present
        string? requestBody = null;
        if (request.ContentLength > 0 && request.ContentLength < 10240) // Max 10KB
        {
            request.EnableBuffering();
            using var reader = new StreamReader(
                request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);

            requestBody = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            // Redact sensitive data
            requestBody = RedactSensitiveData(requestBody);
        }

        var userId = context.User?.Identity?.IsAuthenticated == true
            ? context.User.FindFirst("sub")?.Value ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "authenticated"
            : "anonymous";

        var logData = new
        {
            RequestId = requestId,
            Timestamp = DateTime.UtcNow,
            ClientIp = clientIp,
            UserId = userId,
            Method = request.Method,
            Path = request.Path.Value,
            QueryString = request.QueryString.HasValue ? RedactSensitiveData(request.QueryString.Value) : null,
            ContentType = request.ContentType,
            ContentLength = request.ContentLength,
            UserAgent = request.Headers.UserAgent.ToString(),
            Body = requestBody
        };

        _logger.LogInformation(
            "[REQUEST] {RequestId} | {Method} {Path} | IP: {ClientIp} | User: {UserId}",
            requestId, request.Method, request.Path, clientIp, userId);

        // Log detailed info at Debug level
        if (_logger.IsEnabled(LogLevel.Debug) && requestBody != null)
        {
            _logger.LogDebug("[REQUEST BODY] {RequestId} | {Body}", requestId, requestBody);
        }
    }

    private async Task LogResponse(HttpContext context, string requestId, string clientIp, long elapsedMs)
    {
        var response = context.Response;

        // Read response body for error responses
        string? responseBody = null;
        if (response.StatusCode >= 400 && response.Body.CanSeek)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(response.Body, leaveOpen: true);
            responseBody = await reader.ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            // Redact sensitive data
            responseBody = RedactSensitiveData(responseBody);

            // Truncate if too long
            if (responseBody?.Length > 1000)
                responseBody = responseBody[..1000] + "... [truncated]";
        }

        var logLevel = response.StatusCode switch
        {
            >= 500 => LogLevel.Error,
            >= 400 => LogLevel.Warning,
            _ => LogLevel.Information
        };

        _logger.Log(logLevel,
            "[RESPONSE] {RequestId} | {StatusCode} | {ElapsedMs}ms | {Method} {Path}",
            requestId, response.StatusCode, elapsedMs,
            context.Request.Method, context.Request.Path);

        // Log response body for errors
        if (responseBody != null && _logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("[RESPONSE BODY] {RequestId} | {Body}", requestId, responseBody);
        }

        // Log slow requests
        if (elapsedMs > 5000)
        {
            _logger.LogWarning(
                "[SLOW REQUEST] {RequestId} | {Method} {Path} | {ElapsedMs}ms | IP: {ClientIp}",
                requestId, context.Request.Method, context.Request.Path, elapsedMs, clientIp);
        }
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded headers (when behind proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // Take the first IP in the chain (original client)
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
                return ips[0].Trim();
        }

        // Check X-Real-IP header
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
            return realIp;

        // Fallback to connection remote IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static string? RedactSensitiveData(string? data)
    {
        if (string.IsNullOrEmpty(data))
            return data;

        // Redact sensitive fields in JSON
        var redacted = SensitiveDataRegex.Replace(data, "$1\"[REDACTED]\"");

        // Redact Authorization header values
        redacted = Regex.Replace(redacted,
            @"(Bearer\s+)[A-Za-z0-9\-_]+\.[A-Za-z0-9\-_]+\.[A-Za-z0-9\-_]+",
            "$1[REDACTED]",
            RegexOptions.IgnoreCase);

        return redacted;
    }
}

/// <summary>
/// Extension method for easy middleware registration
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
