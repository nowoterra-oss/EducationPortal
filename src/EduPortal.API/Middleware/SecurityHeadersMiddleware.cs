namespace EduPortal.API.Middleware;

/// <summary>
/// Adds security headers to all HTTP responses to protect against common web vulnerabilities.
/// Implements Helmet.js-like protections for ASP.NET Core.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;

    public SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Remove server identification headers
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");

        // Add security headers
        var headers = context.Response.Headers;

        // Prevent MIME type sniffing
        headers["X-Content-Type-Options"] = "nosniff";

        // Prevent clickjacking attacks
        headers["X-Frame-Options"] = "DENY";

        // Enable XSS filter in browsers (legacy, but still useful for older browsers)
        headers["X-XSS-Protection"] = "1; mode=block";

        // Prevent content type from being changed
        headers["X-Download-Options"] = "noopen";

        // Control DNS prefetching
        headers["X-DNS-Prefetch-Control"] = "off";

        // Referrer policy - only send referrer for same origin
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Permissions Policy - restrict browser features
        headers["Permissions-Policy"] = "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()";

        // Content Security Policy
        // More permissive in development for Swagger UI
        if (_environment.IsDevelopment())
        {
            headers["Content-Security-Policy"] =
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data: https:; " +
                "font-src 'self' data:; " +
                "connect-src 'self' http://localhost:* https://localhost:*; " +
                "frame-ancestors 'none';";
        }
        else
        {
            headers["Content-Security-Policy"] =
                "default-src 'self'; " +
                "script-src 'self'; " +
                "style-src 'self'; " +
                "img-src 'self' data: https:; " +
                "font-src 'self'; " +
                "connect-src 'self'; " +
                "frame-ancestors 'none'; " +
                "upgrade-insecure-requests;";
        }

        // HTTP Strict Transport Security (HSTS) - only in production
        if (!_environment.IsDevelopment())
        {
            // max-age: 1 year, includeSubDomains, preload
            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
        }

        // Cross-Origin policies
        headers["Cross-Origin-Opener-Policy"] = "same-origin";
        headers["Cross-Origin-Resource-Policy"] = "same-origin";

        // Cache control for sensitive data
        if (context.Request.Path.StartsWithSegments("/api/auth") ||
            context.Request.Path.StartsWithSegments("/api/user"))
        {
            headers["Cache-Control"] = "no-store, no-cache, must-revalidate, proxy-revalidate";
            headers["Pragma"] = "no-cache";
            headers["Expires"] = "0";
        }

        await _next(context);
    }
}

/// <summary>
/// Extension method for easy middleware registration
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
