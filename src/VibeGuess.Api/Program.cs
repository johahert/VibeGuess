using Microsoft.EntityFrameworkCore;
using VibeGuess.Infrastructure.Data;
using VibeGuess.Infrastructure.Extensions;
using VibeGuess.Spotify.Authentication.Extensions;
using Microsoft.AspNetCore.Authentication;
using VibeGuess.Api.Middleware;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add authentication
builder.Services.AddAuthentication("Test")
    .AddScheme<AuthenticationSchemeOptions, VibeGuess.Api.Authentication.TestAuthenticationHandler>(
        "Test", options => { });

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
}

app.UseHttpsRedirection();

// Add error handling middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

app.Run();

public partial class Program { }
