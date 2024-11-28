using System.Configuration;
using System.Diagnostics;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieApp.Data;
using MovieApp.Models;

namespace MovieApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ILogger<MovieController> _Movielogger;
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        // Constructor - pass services to methods
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IHttpClientFactory httpClient, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _httpClientFactory = httpClient;
            _configuration = configuration;
        }

        // Return index and pass all movies in the database as list
        public IActionResult Index()
        {
            var allMovies = _context.Movies.ToList();
            return View(allMovies);
        }


        // Authorized page - return only for users with the role "StoreManager".
        // Return the page to submit a new movie or edit an existing one. If an ID is passed along to the page (meaning we are editing a page)
        // Then find the movie with that ID in the database, else initialize a new movie object to pass to the Form method.
        [Authorize(Roles = "StoreManager")]
        public IActionResult CreateEditMovie(int? id)
        {
            var movie = id != null
                ? _context.Movies.FirstOrDefault(movie => movie.Id == id)
                : new Movie();

            return View(movie);
        }

        // Same as index. Parse movies from the database into a list and pass them to the page
        public IActionResult ManagerOverview()
        {
            var allMovies = _context.Movies.ToList();
            return View(allMovies);
        }

        // First find the movie in the database. Then find the relevant information returned by the API to that movie after it was created. Parse both movie models into
        // the view model and return it along with the page.
        public IActionResult MovieDetails(int id, string name)
        {
            var movieController = new MovieController(_Movielogger, _context, _httpClientFactory, _configuration);

            var movie = _context.Movies.SingleOrDefault(mov => mov.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            var movieData = _context.MoviesAPI.SingleOrDefault(mov => mov.Title == name);
            if (movieData == null)
            {
                return NotFound();
            }

            var viewModel = new MovieDetailsViewModel
            {
                Movie = movie,
                MovieAPI = movieData
            };

            

            return View(viewModel);
        }


        // Error page
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}