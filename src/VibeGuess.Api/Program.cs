﻿using Microsoft.EntityFrameworkCore;
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
