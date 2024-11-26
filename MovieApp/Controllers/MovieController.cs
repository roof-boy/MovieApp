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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public MovieController(ILogger<MovieController> logger, ApplicationDbContext context, IHttpClientFactory httpClient, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _httpClientFactory = httpClient;
            _configuration = configuration;
        }

        public IActionResult CreateEditMovieForm(Movie movie, int? id)
        {
            if (movie.Id == 0)
            {
                var movieData = GetMovieData(movie.Name);
                movie.ThumbUrl = movieData.Result.ThumbUrl;

                _context.MoviesAPI.Add(movieData.Result);

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
            string url = $"https://www.omdbapi.com/?apikey={apiKey}&t={name}&plot=full";

            var httpClient = _httpClientFactory.CreateClient();

            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var jsonDocument = JsonDocument.Parse(jsonResponse);


            var movieData = jsonDocument.RootElement; // Assume we take the first result

            var genreRaw = movieData.GetProperty("Genre").GetString();

            string[] genreProcessed = genreRaw.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();

            if (movieData.ValueKind == JsonValueKind.Undefined)
            {
                return null; // Handle no results
            }

            return new MovieAPI
            {
                Title = movieData.GetProperty("Title").GetString(),
                Plot = movieData.GetProperty("Plot").GetString(),
                Genre = genreProcessed,
                ThumbUrl = movieData.GetProperty("Poster").GetString(),
            };
        }
    }
}