using AspNetCoreRateLimit;
using EduPortal.API.Middleware;
using EduPortal.Application;
using EduPortal.Application.Validators.Auth;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure;
using EduPortal.Infrastructure.Configuration;
using EduPortal.Infrastructure.Data;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Hide ASP.NET and Kestrel version headers for security
builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});

// Get and log connection string
var connectionString = builder.Configuration.GetConnectionString("ConnectionString");
Console.WriteLine("=======================================================");
Console.WriteLine($"[CONNECTION STRING] Using database connection:");
Console.WriteLine($"[CONNECTION STRING] {connectionString}");
Console.WriteLine("=======================================================");

// Add DbContext with retry logic for transient failures
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        // Enable retry on failure for transient errors
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);

        // Set command timeout
        sqlOptions.CommandTimeout(60);
    }));

// Configure EmailSettings
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

// Add Infrastructure Services (Repositories)
builder.Services.AddInfrastructure();

// Add Application Services
builder.Services.AddApplication();

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not configured"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Add CORS (Configuration-based)
var corsSettings = builder.Configuration.GetSection("Cors");
var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
var allowedMethods = corsSettings.GetSection("AllowedMethods").Get<string[]>() ?? new[] { "GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS" };
var allowedHeaders = corsSettings.GetSection("AllowedHeaders").Get<string[]>() ?? new[] { "*" };
var allowCredentials = corsSettings.GetValue<bool>("AllowCredentials", true);
var maxAge = corsSettings.GetValue<int>("MaxAge", 3600);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                // Development: Allow all localhost origins + configured origins
                policy.SetIsOriginAllowed(origin =>
                {
                    var uri = new Uri(origin);
                    if (uri.Host == "localhost" || uri.Host == "127.0.0.1")
                        return true;
                    // Also allow configured origins in development
                    return allowedOrigins.Contains(origin);
                });
            }
            else
            {
                // Production: Only allow configured origins
                policy.WithOrigins(allowedOrigins);
            }

            // Apply configured methods
            if (allowedMethods.Contains("*"))
                policy.AllowAnyMethod();
            else
                policy.WithMethods(allowedMethods);

            // Apply configured headers
            if (allowedHeaders.Contains("*"))
                policy.AllowAnyHeader();
            else
                policy.WithHeaders(allowedHeaders);

            // Allow credentials if configured
            if (allowCredentials)
                policy.AllowCredentials();

            // Set preflight max age
            policy.SetPreflightMaxAge(TimeSpan.FromSeconds(maxAge));
        });
});

// Add Controllers with FluentValidation and JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as strings for better API compatibility
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();

// Add Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
builder.Services.AddInMemoryRateLimiting();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EduPortal API",
        Version = "v1",
        Description = "Education Portal Backend API"
    });

    // JWT configuration for Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Initialize Database with Seed Data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbInitializer.InitializeAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// ==========================================
// Configure the HTTP request pipeline
// Middleware order is critical for security!
// ==========================================

// 1. Security Headers (first - applies to ALL responses)
app.UseSecurityHeaders();

// 2. Request Logging (early - to capture all requests)
app.UseRequestLogging();

// 3. Exception Handlers
app.UseValidationExceptionHandler();

// 4. HTTPS Redirection (before other middleware in production)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

// 5. Swagger (development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 6. CORS (before rate limiting and auth)
app.UseCors("AllowAngular");

// 7. Rate Limiting
app.UseIpRateLimiting();

// 8. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 9. Audit Middleware (after auth, to capture user info)
app.UseMiddleware<AuditMiddleware>();

// 10. Health Check Endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName,
    version = "1.0.0"
})).AllowAnonymous();

// 11. Endpoints
app.MapControllers();

app.Run();
