using Microsoft.Extensions.Options;
using MongoDB.Driver;

public class MongoMovieService : IMovieService
{

    private readonly IOptions<DatabaseSettings> options;
    private readonly IMongoCollection<Movie> movieCollection;

    public MongoMovieService(IOptions<DatabaseSettings> options)
    {
        this.options = options;
        var mongoClient = new MongoClient(options.Value.ConnectionString);
        var database = mongoClient.GetDatabase("mydatabase");
        var movieCollection = database.GetCollection<Movie>("movies");
        this.movieCollection = movieCollection;
    }

    public IResult Check()
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
    }

    public IResult Create(Movie movie)
    {
        if (string.IsNullOrEmpty(movie.Id) || movieCollection.Find(m => m.Id == movie.Id).Any())
        {
            return Results.Conflict("Movie with this ID already exists.");
        }
        movieCollection.InsertOne(movie);
        return Results.Ok(movie);
    }

    public IResult Get()
    {
        var movies = movieCollection.Find(_ => true).ToList();
        return Results.Ok(movies);
    }

    public IResult Get(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return Results.NotFound("Movie not found.");
        }
        var movie = movieCollection.Find(m => m.Id == id).FirstOrDefault();
        if (movie == null)
        {
            return Results.NotFound("Movie not found.");
        }
        return Results.Ok(movie);
    }

    public IResult Update(string id, Movie movie)
    {
        if (string.IsNullOrEmpty(id))
        {
            return Results.NotFound("Movie not found.");
        }
        var result = movieCollection.ReplaceOne(m => m.Id == id, movie);
        if (result.MatchedCount == 0)
        {
            return Results.NotFound("Movie not found.");
        }
        return Results.Ok(movie);
    }

    public IResult Remove(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return Results.NotFound("Movie not found.");
        }
        var result = movieCollection.DeleteOne(m => m.Id == id);
        if (result.DeletedCount == 0)
        {
            return Results.NotFound("Movie not found.");
        }
        return Results.Ok("Movie deleted successfully.");
    }
}