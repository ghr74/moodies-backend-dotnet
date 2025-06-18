using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.Sqlite;
using moodies_backend.Model;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors();

var connectionString = builder.Configuration.GetConnectionString("Default");

app.MapGet("/search", async (string q, int skip = 0, int take = 20) =>
{
    await using var connection = new SqliteConnection(connectionString);
    await connection.OpenAsync();

    const string getQuery = """
                            SELECT idx AS id, title_adj AS title, score AS score
                            FROM movies_fts
                            WHERE movies_fts MATCH @q
                            ORDER BY score DESC
                            LIMIT @take
                            OFFSET @skip
                            """;

    var result = await connection.QueryAsync<MovieSearchResult>(getQuery, new { q, skip, take });

    return TypedResults.Ok(result);
}).WithName("SearchMovies").WithOpenApi();

app.MapHealthChecks("/healthc").RequireHost("*:4318");

app.Run();