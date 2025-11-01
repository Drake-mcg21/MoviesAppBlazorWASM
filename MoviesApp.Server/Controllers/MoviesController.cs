using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MoviesApp.Server.Hubs;
using MoviesApp.Shared.DTOs;
using MoviesApp.Server.Protos;
namespace MoviesApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly MoviesService.MoviesServiceClient _grpcClient;
        private readonly IHubContext<MovieHub> _hub;
        private readonly ILogger<MoviesController> _logger;

        public MoviesController(
            MoviesService.MoviesServiceClient grpcClient,
            IHubContext<MovieHub> hub,
            ILogger<MoviesController> logger)
        {
            _grpcClient = grpcClient;
            _hub = hub;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovieDto>>> GetMovies()
        {
            try
            {
                var response = await _grpcClient.GetMoviesAsync(new GetMoviesRequest());

                var movies = response.Movies.Select(m => new MovieDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    Genre = m.Genre,
                    ReleaseDate = DateTime.Parse(m.ReleaseDate),
                    BoxOfficeSales = m.BoxOfficeSales
                }).ToList();

                return Ok(movies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movies");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MovieDto>> GetMovie(int id)
        {
            try
            {
                var response = await _grpcClient.GetMovieAsync(new GetMovieRequest { Id = id });

                if (!response.Found)
                {
                    return NotFound();
                }

                var movieDto = new MovieDto
                {
                    Id = response.Movie.Id,
                    Title = response.Movie.Title,
                    Genre = response.Movie.Genre,
                    ReleaseDate = DateTime.Parse(response.Movie.ReleaseDate),
                    BoxOfficeSales = response.Movie.BoxOfficeSales
                };

                return Ok(movieDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movie with id {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<MovieDto>> CreateMovie(CreateMovieDto createMovieDto)
        {
            try
            {
                var request = new CreateMovieRequest
                {
                    Title = createMovieDto.Title,
                    Genre = createMovieDto.Genre,
                    ReleaseDate = createMovieDto.ReleaseDate.ToString("yyyy-MM-dd"),
                    BoxOfficeSales = createMovieDto.BoxOfficeSales
                };

                var response = await _grpcClient.CreateMovieAsync(request);

                var movieDto = new MovieDto
                {
                    Id = response.Movie.Id,
                    Title = response.Movie.Title,
                    Genre = response.Movie.Genre,
                    ReleaseDate = DateTime.Parse(response.Movie.ReleaseDate),
                    BoxOfficeSales = response.Movie.BoxOfficeSales
                };


                // Also send via SignalR
                await _hub.Clients.All.SendAsync("MovieCreated", movieDto);

                return CreatedAtAction(nameof(GetMovie), new { id = movieDto.Id }, movieDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating movie");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMovie(int id, UpdateMovieDto updateMovieDto)
        {
            try
            {
                var request = new UpdateMovieRequest
                {
                    Id = id,
                    Title = updateMovieDto.Title,
                    Genre = updateMovieDto.Genre,
                    ReleaseDate = updateMovieDto.ReleaseDate.ToString("yyyy-MM-dd"),
                    BoxOfficeSales = updateMovieDto.BoxOfficeSales
                };

                var response = await _grpcClient.UpdateMovieAsync(request);

                if (!response.Found)
                {
                    return NotFound();
                }

                var movieDto = new MovieDto
                {
                    Id = response.Movie.Id,
                    Title = response.Movie.Title,
                    Genre = response.Movie.Genre,
                    ReleaseDate = DateTime.Parse(response.Movie.ReleaseDate),
                    BoxOfficeSales = response.Movie.BoxOfficeSales
                };


                // Also send via SignalR
                await _hub.Clients.All.SendAsync("MovieUpdated", movieDto);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating movie with id {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            try
            {
                var response = await _grpcClient.DeleteMovieAsync(new DeleteMovieRequest { Id = id });

                if (!response.Found)
                {
                    return NotFound();
                }

                // Send via SignalR
                await _hub.Clients.All.SendAsync("MovieDeleted", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting movie with id {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}