using Microsoft.AspNetCore.Mvc;
using MovieApp.Data;
using MovieApp.Models;

namespace MovieApp.Controllers
{
    public class MovieController : Controller
    {
        private readonly ILogger<MovieController> _logger;
        private readonly ApplicationDbContext _context;

        public MovieController(ILogger<MovieController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpPost]
        public IActionResult CreateMovieForm(Movie movie)
        {

            // Save the movie to the database (example, not implemented)
             _context.Movies.Add(movie);
             _context.SaveChanges();

            return RedirectToAction("Index"); // Redirect to a list or detail page
        }
    }
}
