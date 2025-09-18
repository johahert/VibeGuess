using Microsoft.EntityFrameworkCore;
using VibeGuess.Infrastructure.Data;
using VibeGuess.Infrastructure.Extensions;
using VibeGuess.Spotify.Authentication.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Map controllers
app.MapControllers();

app.Run();

public partial class Program { }
