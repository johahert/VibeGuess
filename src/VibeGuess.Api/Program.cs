using Microsoft.EntityFrameworkCore;
using VibeGuess.Infrastructure.Data;
using VibeGuess.Infrastructure.Extensions;
using VibeGuess.Spotify.Authentication.Extensions;
using Microsoft.AspNetCore.Authentication;
using VibeGuess.Api.Middleware;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add controllers with improved JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Swagger: add JWT Bearer support in the UI
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Enhanced CORS configuration for React development
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactDevelopment", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",     // Create React App default
                "http://localhost:5173",     // Vite default (your React app)
                "http://127.0.0.1:5173",     // Alternative localhost
                "https://localhost:3000",    // HTTPS variants
                "https://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromHours(1)); // Cache preflight responses
    });

    // Fallback permissive policy for development environments
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configure JWT settings and service
builder.Services.Configure<VibeGuess.Api.Security.JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<VibeGuess.Api.Security.IJwtService, VibeGuess.Api.Security.JwtService>();

// Add authentication: register both Test scheme (for integration tests) and JWT Bearer
var isTestEnv = string.Equals(builder.Environment.EnvironmentName, "Test", StringComparison.OrdinalIgnoreCase);
if (isTestEnv)
{
    // In test environment, keep Test scheme as default
    builder.Services.AddAuthentication("Test")
        .AddScheme<AuthenticationSchemeOptions, VibeGuess.Api.Authentication.TestAuthenticationHandler>(
            "Test", options => { })
        .AddJwtBearer("Bearer", options =>
        {
            var jwtSection = builder.Configuration.GetSection("Jwt");
            var secret = jwtSection.GetValue<string>("Secret");
            var issuer = jwtSection.GetValue<string>("Issuer");
            var audience = jwtSection.GetValue<string>("Audience");

            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(secret ?? string.Empty))
            };
            options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger("JwtAuth");
                    logger.LogError(context.Exception, "JWT authentication failed: {Message}", context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger("JwtAuth");
                    var sub = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    var spotifyId = context.Principal?.FindFirst("spotify_user_id")?.Value;
                    logger.LogInformation("JWT validated for user id {sub}, spotify id {spotifyId}", sub, spotifyId);
                    return Task.CompletedTask;
                }
            };
        });
}
else
{
    // In non-test (dev/prod), use Bearer as default authenticate/challenge scheme
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "Bearer";
        options.DefaultChallengeScheme = "Bearer";
    })
    .AddJwtBearer("Bearer", options =>
    {
        var jwtSection = builder.Configuration.GetSection("Jwt");
        var secret = jwtSection.GetValue<string>("Secret");
        var issuer = jwtSection.GetValue<string>("Issuer");
        var audience = jwtSection.GetValue<string>("Audience");

        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(secret ?? string.Empty))
        };
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtAuth");
                logger.LogError(context.Exception, "JWT authentication failed: {Message}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtAuth");
                var sub = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var spotifyId = context.Principal?.FindFirst("spotify_user_id")?.Value;
                logger.LogInformation("JWT validated for user id {sub}, spotify id {spotifyId}", sub, spotifyId);
                return Task.CompletedTask;
            }
        };
    })
    .AddScheme<AuthenticationSchemeOptions, VibeGuess.Api.Authentication.TestAuthenticationHandler>(
        "Test", options => { });
}

builder.Services.AddAuthorization();

// Add database
builder.Services.AddDbContext<VibeGuessDbContext>(options =>
    options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=VibeGuessDb;Trusted_Connection=true"));

// Add repositories
builder.Services.AddRepositories();

// Add authentication services
builder.Services.AddSpotifyAuthentication(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

// CRITICAL: CORS must be the first middleware after developer exception page
app.UseCors(app.Environment.IsDevelopment() ? "AllowAll" : "ReactDevelopment");

// HTTPS redirection comes after CORS to avoid redirect issues with preflight
app.UseHttpsRedirection();

// Add error handling middleware after CORS
app.UseMiddleware<ErrorHandlingMiddleware>();

// Authentication/Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Log the configuration for debugging
if (app.Environment.IsDevelopment())
{
    app.Logger.LogInformation("API is running on:");
    app.Logger.LogInformation("- HTTP: http://localhost:5087");
    app.Logger.LogInformation("- HTTPS: https://localhost:7009");
    app.Logger.LogInformation("CORS is enabled for React development origins");
}

app.Run();

public partial class Program { }
