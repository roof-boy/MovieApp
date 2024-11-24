using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
        public IActionResult CreateMovieForm(Movie movie, int? id)
        {
            if (movie.Id == 0)
            {
                _context.Movies.Add(movie);
            } else
            {
                _context.Movies.Update(movie);
            }
             
             _context.SaveChanges();

            return RedirectToAction("Index",  new { controller = "Home" }); // Redirect to a list or detail page
        }

        public IActionResult DeleteMovie(int id)
        {
            var movieInDb = _context.Movies.SingleOrDefault(movie => movie.Id == id);
            if (movieInDb == null)
            {
                return NotFound();
            }

            _context.Movies.Remove(movieInDb);
            _context.SaveChanges();

            return RedirectToAction("ManagerOverview", new { controller = "Home" });
        }
    }
}
