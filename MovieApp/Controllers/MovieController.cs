using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MovieApp.Data;
using MovieApp.Models;
using System.Text.Json;

namespace MovieApp.Controllers
{
    public class MovieController : Controller
    {
        private readonly ILogger<MovieController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public MovieController(ILogger<MovieController> logger, ApplicationDbContext context, HttpClient httpClient, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration;
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

        public async Task<MovieAPI> GetMovieData(string name)
        {
            string apiKey = _configuration.GetValue<string>("ApiKey");
            string url = $"https://www.omdbapi.com/?apikey={apiKey}&t={name}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var jsonDocument = JsonDocument.Parse(jsonResponse);
            var movieData = jsonDocument.RootElement
                .GetProperty("results")
                .EnumerateArray()
                .FirstOrDefault(); // Assume we take the first result

            if (movieData.ValueKind == JsonValueKind.Undefined)
            {
                return null; // Handle no results
            }

            return new MovieAPI
            {
                Title = movieData.GetProperty("title").GetString(),
                Plot = movieData.GetProperty("plot").GetString(),
                Genre = movieData.GetProperty("Genre").GetString()
            };
        }
    }
}