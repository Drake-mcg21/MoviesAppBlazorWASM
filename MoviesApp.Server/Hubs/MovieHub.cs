using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MoviesApp.Server.Data;
using MoviesApp.Shared.DTOs;

namespace MoviesApp.Server.Hubs
{
    public class MovieHub : Hub
    {
        private readonly MoviesDbContext _context;

        public MovieHub(MoviesDbContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
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

            // Send all current movies to just the connecting client
            await Clients.Caller.SendAsync("AllMovies", movies);

            await base.OnConnectedAsync();
        }
    }
}
