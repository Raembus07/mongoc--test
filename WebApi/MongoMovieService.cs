using Microsoft.Extensions.Options;
using MongoDB.Driver;

public class MongoMovieService : IMovieService
{
    List<Movie> movies = new List<Movie>();

    private readonly IOptions<DatabaseSettings> _options;

    public MongoMovieService(IOptions<DatabaseSettings> options)
    {
        _options = options;
    }

    public IResult Check()
    {
        var mongoDbConnectionString = _options.Value.ConnectionString;

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
    }

    public IResult Create(Movie movie)
    {
        if (movies.Any(m => m.Id == movie.Id))
        {
            return Results.Conflict("Movie with the same ID already exists.");
        }
        movies.Add(movie);
        return Results.Ok(movie);
    }

    public IResult Get()
    {
        return Results.Ok(movies);
    }

    public IResult Get(string id)
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
    }

    public IResult Update(string id, Movie movie)
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
    }
    
    public IResult Remove(string id)
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
    }
}