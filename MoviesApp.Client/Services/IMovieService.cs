using MoviesApp.Shared.DTOs;

namespace MoviesApp.Client.Services
{
    public interface IMovieService
    {
        Task<List<MovieDto>> GetMoviesAsync();
        Task<MovieDto?> GetMovieAsync(int id);
        Task<bool> CreateMovieAsync(CreateMovieDto movie);
        Task<bool> UpdateMovieAsync(int id, UpdateMovieDto movie);
        Task<bool> DeleteMovieAsync(int id);
    }
}
