using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using MoviesApp.Server.Protos;
using MoviesApp.Server.Data;
using MoviesApp.Shared.Models;
using System.Globalization;

namespace MoviesApp.Server.Services
{
    public class MoviesGrpcService : MoviesService.MoviesServiceBase
    {
        private readonly MoviesDbContext _context;
        private readonly ILogger<MoviesGrpcService> _logger;

        public MoviesGrpcService(MoviesDbContext context, ILogger<MoviesGrpcService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public override async Task<GetMoviesResponse> GetMovies(GetMoviesRequest request, ServerCallContext context)
        {
            try
            {
                var movies = await _context.Movies.ToListAsync();
                var response = new GetMoviesResponse();

                foreach (var movie in movies)
                {
                    response.Movies.Add(new MovieMessage
                    {
                        Id = movie.Id,
                        Title = movie.Title,
                        Genre = movie.Genre,
                        ReleaseDate = movie.ReleaseDate.ToString("yyyy-MM-dd"),
                        BoxOfficeSales = movie.BoxOfficeSales?.ToString()
                    });
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movies");
                throw new RpcException(new Status(StatusCode.Internal, "Internal error occurred"));
            }
        }

        public override async Task<GetMovieResponse> GetMovie(GetMovieRequest request, ServerCallContext context)
        {
            try
            {
                var movie = await _context.Movies.FindAsync(request.Id);

                if (movie == null)
                {
                    return new GetMovieResponse { Found = false };
                }

                return new GetMovieResponse
                {
                    Found = true,
                    Movie = new MovieMessage
                    {
                        Id = movie.Id,
                        Title = movie.Title,
                        Genre = movie.Genre,
                        ReleaseDate = movie.ReleaseDate.ToString("yyyy-MM-dd"),
                        BoxOfficeSales = movie.BoxOfficeSales?.ToString()
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movie with id {Id}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, "Internal error occurred"));
            }
        }

        public override async Task<CreateMovieResponse> CreateMovie(CreateMovieRequest request, ServerCallContext context)
        {
            try
            {
                if (!DateTime.TryParse(request.ReleaseDate, out var releaseDate))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid release date format"));
                }

                if (!decimal.TryParse(request.BoxOfficeSales, NumberStyles.Float, CultureInfo.InvariantCulture, out var boxOfficeSales))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid box office sales format"));
                }

                var movie = new Movie
                {
                    Title = request.Title,
                    Genre = request.Genre,
                    ReleaseDate = releaseDate,
                    BoxOfficeSales = boxOfficeSales.ToString()
                };

                _context.Movies.Add(movie);
                await _context.SaveChangesAsync();

                return new CreateMovieResponse
                {
                    Movie = new MovieMessage
                    {
                        Id = movie.Id,
                        Title = movie.Title,
                        Genre = movie.Genre,
                        ReleaseDate = movie.ReleaseDate.ToString("yyyy-MM-dd"),
                        BoxOfficeSales = movie.BoxOfficeSales.ToString()
                    }
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating movie");
                throw new RpcException(new Status(StatusCode.Internal, "Internal error occurred"));
            }
        }

        public override async Task<UpdateMovieResponse> UpdateMovie(UpdateMovieRequest request, ServerCallContext context)
        {
            try
            {
                var movie = await _context.Movies.FindAsync(request.Id);

                if (movie == null)
                {
                    return new UpdateMovieResponse { Found = false };
                }

                if (!DateTime.TryParse(request.ReleaseDate, out var releaseDate))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid release date format"));
                }

                if (!decimal.TryParse(request.BoxOfficeSales, NumberStyles.Float, CultureInfo.InvariantCulture, out var boxOfficeSales))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid box office sales format"));
                }

                movie.Title = request.Title;
                movie.Genre = request.Genre;
                movie.ReleaseDate = releaseDate;
                movie.BoxOfficeSales = boxOfficeSales.ToString();

                await _context.SaveChangesAsync();

                return new UpdateMovieResponse
                {
                    Found = true,
                    Movie = new MovieMessage
                    {
                        Id = movie.Id,
                        Title = movie.Title,
                        Genre = movie.Genre,
                        ReleaseDate = movie.ReleaseDate.ToString("yyyy-MM-dd"),
                        BoxOfficeSales = movie.BoxOfficeSales.ToString()
                    }
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating movie with id {Id}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, "Internal error occurred"));
            }
        }

        public override async Task<DeleteMovieResponse> DeleteMovie(DeleteMovieRequest request, ServerCallContext context)
        {
            try
            {
                var movie = await _context.Movies.FindAsync(request.Id);

                if (movie == null)
                {
                    return new DeleteMovieResponse { Found = false };
                }

                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();

                return new DeleteMovieResponse { Found = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting movie with id {Id}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, "Internal error occurred"));
            }
        }
    }
}