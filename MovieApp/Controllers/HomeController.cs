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

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IHttpClientFactory httpClient, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _httpClientFactory = httpClient;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var allMovies = _context.Movies.ToList();
            return View(allMovies);
        }

        [Authorize(Roles = "StoreManager")]
        public IActionResult CreateEditMovie(int? id)
        {
            var movie = id != null
                ? _context.Movies.FirstOrDefault(movie => movie.Id == id)
                : new Movie();

            return View(movie);
        }

        public IActionResult ManagerOverview()
        {
            var allMovies = _context.Movies.ToList();
            return View(allMovies);
        }

        public IActionResult MovieDetails(int id, string name)
        {
            var movieController = new MovieController(_Movielogger, _context, _httpClientFactory, _configuration);

            var movieData = movieController.GetMovieData(name).Result;

            var movie = _context.Movies.SingleOrDefault(mov => mov.Id == id);
            if (movie == null)
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}