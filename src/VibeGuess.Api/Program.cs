using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpOverrides;
using System.Text.Json;
using System.Security.Cryptography;
using VibeGuess.Infrastructure.Data;
using VibeGuess.Infrastructure.Extensions;
using VibeGuess.Spotify.Authentication.Extensions;
using VibeGuess.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);
var enableSwaggerEverywhere = builder.Configuration.GetValue<bool>("EnableSwagger");

// Controllers & JSON
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.JsonSerializerOptions.WriteIndented = true;
});

builder.Services.AddEndpointsApiExplorer();
// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Add header: Authorization: Bearer <token>",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        { new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            { Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
          Array.Empty<string>() }
    });
});

// Enhanced CORS configuration for React development and SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactDevelopment", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "http://127.0.0.1:5173",
                "https://localhost:3000",
                "https://localhost:5173",
                "http://127.0.0.1:8080",
                "https://vibeguess.on-forge.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()  // Required for SignalR
              .SetPreflightMaxAge(TimeSpan.FromHours(1)); // Cache preflight responses
    });
    options.AddPolicy("AllowAll", p => p.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials());
});

// Forwarded headers for Azure
builder.Services.Configure<ForwardedHeadersOptions>(opts =>
{
    opts.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor;
    opts.KnownNetworks.Clear();
    opts.KnownProxies.Clear();
});


// JWT settings
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "vibeguess";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "vibeguess_clients";
var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
{
    jwtSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    Console.WriteLine("WARNING: Jwt:Secret missing or short; ephemeral secret generated.");
}

builder.Services.Configure<VibeGuess.Api.Security.JwtSettings>(o =>
{
    o.Issuer = jwtIssuer;
    o.Audience = jwtAudience;
    o.Secret = jwtSecret;
    o.ExpiryMinutes = int.TryParse(builder.Configuration["Jwt:ExpiryMinutes"], out var mins) ? mins : 60;
});
builder.Services.AddSingleton<VibeGuess.Api.Security.IJwtService, VibeGuess.Api.Security.JwtService>();

// Domain services
builder.Services.Configure<VibeGuess.Api.Services.OpenAI.OpenAISettings>(builder.Configuration.GetSection("OpenAI"));
builder.Services.AddHttpClient();
builder.Services.AddScoped<VibeGuess.Api.Services.Quiz.IQuizGenerationService, VibeGuess.Api.Services.Quiz.QuizGenerationService>();
builder.Services.AddScoped<VibeGuess.Api.Services.Spotify.ISpotifyApiService, VibeGuess.Api.Services.Spotify.SpotifyApiService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<VibeGuess.Api.Services.Authentication.ICurrentUserSpotifyService, VibeGuess.Api.Services.Authentication.CurrentUserSpotifyService>();

// Authentication
var isTestEnv = string.Equals(builder.Environment.EnvironmentName, "Test", StringComparison.OrdinalIgnoreCase);
if (isTestEnv)
{
    builder.Services.AddAuthentication("Test")
        .AddScheme<AuthenticationSchemeOptions, VibeGuess.Api.Authentication.TestAuthenticationHandler>("Test", _ => { })
        .AddJwtBearer("Bearer", opt =>
        {
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSecret))
            };
        });
}
else
{
    builder.Services.AddAuthentication(o =>
    {
        o.DefaultAuthenticateScheme = "Bearer";
        o.DefaultChallengeScheme = "Bearer";
    })
    .AddJwtBearer("Bearer", opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSecret))
        };
    })
    .AddScheme<AuthenticationSchemeOptions, VibeGuess.Api.Authentication.TestAuthenticationHandler>("Test", _ => { });
}
builder.Services.AddAuthorization();

builder.Services.AddAuthorization();

// Database (requires ConnectionStrings:DefaultConnection)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");
builder.Services.AddDbContext<VibeGuessDbContext>(options =>
    options.UseSqlServer(connectionString));

// Repositories & Spotify auth
builder.Services.AddRepositories();

// Add Redis distributed cache for live sessions (optional for development)
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
var useRedis = !string.IsNullOrEmpty(redisConnectionString) && redisConnectionString != "disabled";

if (useRedis && !builder.Environment.IsDevelopment())
{
    // Production: Use Redis for distributed caching and SignalR backplane
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString!;
        options.InstanceName = "VibeGuess";
    });
    
    builder.Services.AddSignalR()
        .AddStackExchangeRedis(redisConnectionString!);
}
else
{
    // Development: Use in-memory caching and single-server SignalR
    builder.Services.AddMemoryCache();
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSignalR();
    
    if (builder.Environment.IsDevelopment())
    {
        Console.WriteLine("Development mode: Using in-memory caching instead of Redis");
    }
}

// Add live session manager
builder.Services.AddScoped<VibeGuess.Core.Interfaces.ILiveSessionManager, VibeGuess.Infrastructure.Services.LiveSessionManager>();

// Add authentication services
builder.Services.AddSpotifyAuthentication(builder.Configuration);

var app = builder.Build();

// Apply pending EF Core migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<VibeGuessDbContext>();
        db.Database.Migrate();
        app.Logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Failed to apply database migrations.");
        // Optionally rethrow or continue; here we continue to let app start
    }
}

app.UseForwardedHeaders();

if (builder.Environment.IsDevelopment() || enableSwaggerEverywhere)
{
    app.UseSwagger();
    app.UseSwaggerUI();
    if (builder.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();
}

app.UseCors(app.Environment.IsDevelopment() ? "AllowAll" : "ReactDevelopment");
if (app.Environment.IsDevelopment()) app.UseHttpsRedirection();

// Request logging
app.Use(async (ctx, next) =>
{
    var logger = ctx.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("RequestLog");
    logger.LogInformation("Incoming {Method} {Path}", ctx.Request.Method, ctx.Request.Path);
    await next();
    logger.LogInformation("Response {StatusCode} for {Method} {Path}", ctx.Response.StatusCode, ctx.Request.Method, ctx.Request.Path);
});

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => new { status = "ok", message = "VibeGuess API", time = DateTime.UtcNow });
app.MapGet("/health", () => new { status = "healthy", time = DateTime.UtcNow });
app.MapControllers();

// Map SignalR hubs
app.MapHub<VibeGuess.Api.Hubs.HostedQuizHub>("/hubs/hostedquiz");

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
