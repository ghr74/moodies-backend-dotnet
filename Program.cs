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

// Vor builder.Build():
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Nach app.UseHttpsRedirection():
app.UseCors();

app.MapGet("/searchingPreview", async (string search, int skip = 0, int take = 5) =>
{
    await using var connection = new SqliteConnection("Data Source=search.db");
    await connection.OpenAsync();

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