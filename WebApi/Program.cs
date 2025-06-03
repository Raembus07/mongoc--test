using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
var movieDatabaseConfigSection = builder.Configuration.GetSection("DatabaseSettings");
builder.Services.Configure<DatabaseSettings>(movieDatabaseConfigSection);
var app = builder.Build();

List<Movie> movies = new List<Movie>();

app.MapGet("/", () => "Hello World!");

app.MapGet("/check", (Microsoft.Extensions.Options.IOptions<DatabaseSettings> options) =>
{
    var mongoDbConnectionString = options.Value.ConnectionString;

    try
    {
        var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var cancellationToken = cancellationTokenSource.Token;

        var client = new MongoClient(mongoDbConnectionString);

        var databases = client.ListDatabaseNames(cancellationToken).ToList();

        return Results.Ok("Zugriff auf MongoDB ok. Datenbanken: " + string.Join(", ", databases));
    }
    catch (TimeoutException ex)
    {
        return Results.Problem("Fehler: Timeout beim Zugriff auf MongoDB: " + ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem("Fehler beim Zugriff auf MongoDB: " + ex.Message);
    }
});

// Insert Movie
// Wenn das übergebene Objekt eingefügt werden konnte,
// wird es mit Statuscode 200 zurückgegeben.
// Bei Fehler wird Statuscode 409 Conflict zurückgegeben.
app.MapPost("/api/movies", (Movie movie) =>
{
    if (movies.Any(m => m.Id == movie.Id))
    {
        return Results.Conflict("Movie with the same ID already exists.");
    }
    movies.Add(movie);
    return Results.Ok(movie);
});

// Get all Movies
// Gibt alle vorhandenen Movie-Objekte mit Statuscode 200 OK zurück.
app.MapGet("api/movies", () =>
{
    return Results.Ok(movies);
});

// Get Movie by id
// Gibt das gewünschte Movie-Objekt mit Statuscode 200 OK zurück.
// Bei ungültiger id wird Statuscode 404 not found zurückgegeben.
app.MapGet("api/movies/{id}", (string id) =>
{
    if (string.IsNullOrEmpty(id))
    {
        return Results.NotFound("Movie not found.");
    }
    var movie = movies.FirstOrDefault(m => m.Id == id);
    if (movie == null)
    {
        return Results.NotFound("Movie not found.");
    }
    return Results.Ok(movie);
});

// Update Movie
// Gibt das aktualisierte Movie-Objekt zurück.
// Bei ungültiger id wird Statuscode 404 not found zurückgegeben.
app.MapPut("/api/movies/{id}", (string id, Movie movie) =>
{
    if (string.IsNullOrEmpty(id) || movie == null)
    {
        return Results.NotFound("Movie not found.");
    }

    var existingMovie = movies.FirstOrDefault(m => m.Id == id);
    if (existingMovie == null)
    {
        return Results.NotFound("Movie not found.");
    }

    existingMovie.Title = movie.Title;
    existingMovie.Year = movie.Year;
    existingMovie.Summary = movie.Summary;
    existingMovie.Actors = movie.Actors;

    return Results.Ok(existingMovie);
});

// Delete Movie
// Gibt bei erfolgreicher Löschung Statuscode 200 OK zurück.
// Bei ungültiger id wird Statuscode 404 not found zurückgegeben.
app.MapDelete("api/movies/{id}", (string id) =>
{
    if (string.IsNullOrEmpty(id))
    {
        return Results.NotFound("Movie not found.");
    }
    var movie = movies.FirstOrDefault(m => m.Id == id);
    if (movie == null)
    {
        return Results.NotFound("Movie not found.");
    }
    movies.Remove(movie);
    return Results.Ok("Movie deleted successfully.");
});
app.Run();
