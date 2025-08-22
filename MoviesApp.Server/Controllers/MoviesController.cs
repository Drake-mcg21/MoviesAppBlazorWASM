using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MoviesApp.Server.Data;
using MoviesApp.Server.Hubs;
using MoviesApp.Shared.DTOs;
using MoviesApp.Shared.Models;

namespace MoviesApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly MoviesDbContext _context;
        private readonly IHubContext<MovieHub> _hub;
        private readonly IEventPublisher _eventPublisher;

        public MoviesController(MoviesDbContext context, IHubContext<MovieHub> hub, IEventPublisher eventPublisher)
        {
            _context = context;
            _hub = hub;
            _eventPublisher = eventPublisher;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovieDto>>> GetMovies()
        {
            var movies = await _context.Movies
                .Select(m => new MovieDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    Genre = m.Genre,
                    ReleaseDate = m.ReleaseDate,
                    BoxOfficeSales = m.BoxOfficeSales
                })
                .ToListAsync();

            return Ok(movies);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MovieDto>> GetMovie(int id)
        {
            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
            {
                return NotFound();
            }

            var movieDto = new MovieDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Genre = movie.Genre,
                ReleaseDate = movie.ReleaseDate,
                BoxOfficeSales = movie.BoxOfficeSales
            };

            return Ok(movieDto);
        }

        [HttpPost]
        public async Task<ActionResult<MovieDto>> CreateMovie(CreateMovieDto createMovieDto)
        {
            var movie = new Movie
            {
                Title = createMovieDto.Title,
                Genre = createMovieDto.Genre,
                ReleaseDate = createMovieDto.ReleaseDate,
                BoxOfficeSales = createMovieDto.BoxOfficeSales
            };

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            var movieDto = new MovieDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Genre = movie.Genre,
                ReleaseDate = movie.ReleaseDate,
                BoxOfficeSales = movie.BoxOfficeSales
            };

            // Publish event to RabbitMQ
            await _eventPublisher.PublishAsync("MovieCreated", movieDto);

            // Also send via SignalR
            await _hub.Clients.All.SendAsync("MovieCreated", movieDto);

            return CreatedAtAction(nameof(GetMovie), new { id = movie.Id }, movieDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMovie(int id, UpdateMovieDto updateMovieDto)
        {
            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
            {
                return NotFound();
            }

            movie.Title = updateMovieDto.Title;
            movie.Genre = updateMovieDto.Genre;
            movie.ReleaseDate = updateMovieDto.ReleaseDate;
            movie.BoxOfficeSales = updateMovieDto.BoxOfficeSales;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovieExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            var movieDto = new MovieDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Genre = movie.Genre,
                ReleaseDate = movie.ReleaseDate,
                BoxOfficeSales = movie.BoxOfficeSales
            };

            // Publish event to RabbitMQ
            await _eventPublisher.PublishAsync("MovieUpdated", movieDto);

            // Also send via SignalR
            await _hub.Clients.All.SendAsync("MovieUpdated", movieDto);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
            {
                return NotFound();
            }

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            // Publish event to RabbitMQ
            await _eventPublisher.PublishAsync("MovieDeleted", new { Id = id });

            // Also send via SignalR
            await _hub.Clients.All.SendAsync("MovieDeleted", id);

            return NoContent();
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
    }
}