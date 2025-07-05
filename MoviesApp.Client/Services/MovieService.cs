using System.Net.Http.Json;
using MoviesApp.Shared.DTOs;

namespace MoviesApp.Client.Services
{
    public class MovieService : IMovieService
    {
        private readonly HttpClient _httpClient;
        private const string ApiEndpoint = "api/movies";

        public MovieService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<MovieDto>> GetMoviesAsync()
        {
            try
            {
                var movies = await _httpClient.GetFromJsonAsync<List<MovieDto>>(ApiEndpoint);
                return movies ?? new List<MovieDto>();
            }
            catch (Exception)
            {
                return new List<MovieDto>();
            }
        }

        public async Task<MovieDto?> GetMovieAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<MovieDto>($"{ApiEndpoint}/{id}");
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> CreateMovieAsync(CreateMovieDto movie)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(ApiEndpoint, movie);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateMovieAsync(int id, UpdateMovieDto movie)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoint}/{id}", movie);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteMovieAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{ApiEndpoint}/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}