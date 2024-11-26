using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MovieApp.Models
{
    public class MovieAPI
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public string Plot { get; set; }
        public string[] Genre { get; set; }
        public string ThumbUrl { get; set; }
    }
}