using Microsoft.EntityFrameworkCore;
using VibeGuess.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<VibeGuessDbContext>(options =>
    options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=VibeGuessDb;Trusted_Connection=true"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/weatherforecast", () =>
{
    return "Hello World";
});

app.Run();

public partial class Program { }
