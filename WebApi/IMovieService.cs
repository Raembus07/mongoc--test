public interface IMovieService
{
    IResult Check();
    IResult Create(Movie movie);
    IResult Get();
    IResult Get(string id);
    IResult Update(string id, Movie movie);
    IResult Remove(string id);
}