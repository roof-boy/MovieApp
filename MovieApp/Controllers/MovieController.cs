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

        // Constructor - Pass services to methods
        public MovieController(ILogger<MovieController> logger, ApplicationDbContext context, IHttpClientFactory httpClient, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _httpClientFactory = httpClient;
            _configuration = configuration;
        }


        // Handle form data
        public IActionResult CreateEditMovieForm(Movie movie, int? id)
        {
            if (movie.Id == 0)
            {
                // Call GetMovieData in order to fetch OMDb information about the given movie
                var movieData = GetMovieData(movie.Name);
                // Assign ThumbUrl to the one returned by the OMDb API
                movie.ThumbUrl = movieData.Result.ThumbUrl;

                // Add API Results to API Movie Table
                _context.MoviesAPI.Add(movieData.Result);

                // Add movie to database
                _context.Movies.Add(movie);
            } else // If an ID got passed along with the form (meaning we're editing instead of creating a new movie)
            {
                // Update the specific movie with the given information
                _context.Movies.Update(movie);
            }
             
            // Actually save the above actions to the database
             _context.SaveChanges();

            return RedirectToAction("Index",  new { controller = "Home" });
        }

        // Self-Explanatory
        public IActionResult DeleteMovie(int id)
        {
            // Fetch movie from the database based on the given ID
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
            // Get ApiKey from appsettings.json using IConfiguration
            string apiKey = _configuration.GetValue<string>("ApiKey");
            // The URL with concatenated strings attached
            string url = $"https://www.omdbapi.com/?apikey={apiKey}&t={name}&plot=full";

            var httpClient = _httpClientFactory.CreateClient();

            // Send API query
            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            
            // Translate the response to string
            string jsonResponse = await response.Content.ReadAsStringAsync();

            // Parse the response
            var jsonDocument = JsonDocument.Parse(jsonResponse);

            // Provide a variable to access the fields inside the JSON response.
            var movieData = jsonDocument.RootElement;

            // Get the Genre string
            var genreRaw = movieData.GetProperty("Genre").GetString();

            // Organize the different genres into Array entries by removing the commas and using them as splitters.
            string[] genreProcessed = genreRaw.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();

            if (movieData.ValueKind == JsonValueKind.Undefined)
            {
                return null; // Handle no results
            }

            // Pass the model to be saved to data database by CreateEditMovieForm
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