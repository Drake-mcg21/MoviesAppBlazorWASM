using System.ComponentModel.DataAnnotations;

namespace MoviesApp.Shared.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Genre is required")]
        [StringLength(50, ErrorMessage = "Genre cannot exceed 50 characters")]
        public string Genre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Release date is required")]
        public DateTime ReleaseDate { get; set; } = DateTime.Today;

        [Range(0, double.MaxValue, ErrorMessage = "Box office sales must be positive")]
        public string? BoxOfficeSales { get; set; }
    }
}