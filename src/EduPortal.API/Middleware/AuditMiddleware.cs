using EduPortal.Application.DTOs.Audit;
using EduPortal.Application.Interfaces;
using System.Security.Claims;

namespace EduPortal.API.Middleware;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;

    public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuditService auditService)
    {
        // Skip audit for certain paths
        if (ShouldSkipAudit(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var originalBodyStream = context.Response.Body;

        try
        {
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            // Log the request
            await LogRequestAsync(context, auditService);

            // Copy response back to original stream
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in audit middleware");

            // Log the error
            await LogErrorAsync(context, auditService, ex);

            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task LogRequestAsync(HttpContext context, IAuditService auditService)
    {
        try
        {
            var request = context.Request;
            var user = context.User;

            // Determine action based on HTTP method and path
            var action = DetermineAction(request.Method, request.Path);
            var entityType = DetermineEntityType(request.Path);

            // Skip if action or entity type couldn't be determined
            if (string.IsNullOrEmpty(action) || string.IsNullOrEmpty(entityType))
                return;

            var dto = new CreateAuditLogDto
            {
                Action = action,
                EntityType = entityType,
                EntityId = ExtractEntityId(request.Path),
                IpAddress = GetIpAddress(context),
                UserAgent = request.Headers["User-Agent"].ToString(),
                IsSuccessful = context.Response.StatusCode >= 200 && context.Response.StatusCode < 400,
                AdditionalInfo = $"{request.Method} {request.Path}"
            };

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = user.FindFirst(ClaimTypes.Name)?.Value;
            var userEmail = user.FindFirst(ClaimTypes.Email)?.Value;

            await auditService.LogAsync(dto, userId, userName, userEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging request in audit middleware");
        }
    }

    private async Task LogErrorAsync(HttpContext context, IAuditService auditService, Exception ex)
    {
        try
        {
            var request = context.Request;
            var user = context.User;

            var dto = new CreateAuditLogDto
            {
                Action = "Error",
                EntityType = DetermineEntityType(request.Path) ?? "Unknown",
                IpAddress = GetIpAddress(context),
                UserAgent = request.Headers["User-Agent"].ToString(),
                IsSuccessful = false,
                ErrorMessage = ex.Message,
                AdditionalInfo = $"{request.Method} {request.Path}"
            };

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = user.FindFirst(ClaimTypes.Name)?.Value;
            var userEmail = user.FindFirst(ClaimTypes.Email)?.Value;

            await auditService.LogAsync(dto, userId, userName, userEmail);
        }
        catch (Exception logEx)
        {
            _logger.LogError(logEx, "Error logging error in audit middleware");
        }
    }

    private bool ShouldSkipAudit(PathString path)
    {
        var pathValue = path.Value?.ToLower() ?? "";

        // Skip swagger, health checks, and static files
        return pathValue.Contains("/swagger") ||
               pathValue.Contains("/health") ||
               pathValue.Contains("/_framework") ||
               pathValue.Contains("/css") ||
               pathValue.Contains("/js") ||
               pathValue.Contains("/favicon");
    }

    private string DetermineAction(string method, PathString path)
    {
        var pathValue = path.Value?.ToLower() ?? "";

        if (pathValue.Contains("/login"))
            return "Login";
        if (pathValue.Contains("/logout"))
            return "Logout";
        if (pathValue.Contains("/register"))
            return "Register";

        return method.ToUpper() switch
        {
            "GET" => "Read",
            "POST" => "Create",
            "PUT" => "Update",
            "PATCH" => "Update",
            "DELETE" => "Delete",
            _ => "Other"
        };
    }

    private string? DetermineEntityType(PathString path)
    {
        var pathValue = path.Value?.ToLower() ?? "";

        if (pathValue.Contains("/students")) return "Student";
        if (pathValue.Contains("/teachers")) return "Teacher";
        if (pathValue.Contains("/courses")) return "Course";
        if (pathValue.Contains("/payments")) return "Payment";
        if (pathValue.Contains("/homework")) return "Homework";
        if (pathValue.Contains("/attendance")) return "Attendance";
        if (pathValue.Contains("/exams")) return "Exam";
        if (pathValue.Contains("/messages")) return "Message";
        if (pathValue.Contains("/auth")) return "Auth";
        if (pathValue.Contains("/users")) return "User";

        return null;
    }

    private string? ExtractEntityId(PathString path)
    {
        var segments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments == null || segments.Length < 3)
            return null;

        var lastSegment = segments[^1];
        return int.TryParse(lastSegment, out _) ? lastSegment : null;
    }

    private string? GetIpAddress(HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString()
               ?? context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
               ?? context.Request.Headers["X-Real-IP"].FirstOrDefault();
    }
}
