using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.Sqlite;
using moodies_backend.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/searchingPreview", async (string search, string? skip, string? take) =>
{
    await using var connection = new SqliteConnection("Data Source=search.db");
    await connection.OpenAsync();

    if (take == null)
    {
        take = "5";
    }

    if (skip == null)
    {
        skip = "0";
    }

    var searchingPattern = $"%{search}%";

    var getQuery = """
                   SELECT idx, title
                   FROM movies
                   WHERE title
                   LIKE @searchingPattern Limit @take OFFSET @skip
                   """;

    var result = await connection.QueryAsync<MovieSuggestion>(getQuery, new { searchingPattern, skip, take });

    return Results.Ok(result);
}).WithName("GetSearchingPreview").WithOpenApi();

app.Run();