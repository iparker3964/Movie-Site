using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MovieProDemo.Data;
using MovieProDemo.Enums;
using MovieProDemo.Models;
using MovieProDemo.Models.ViewModels;
using MovieProDemo.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MovieProDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly IRemoteMovieService _tmdbMovieService;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db, IRemoteMovieService tmdbMovieService)
        {
            _logger = logger;
            _db = db;
            _tmdbMovieService = tmdbMovieService;
        }

        public async Task<IActionResult> Index()
        {
            const int count = 16;

            var data = new LandingPageVM()
            {
                CustomCollections = await _db.Collection.Include(c => c.MovieCollections).ThenInclude(mc => mc.Movie).ToListAsync(),
                NowPlaying = await _tmdbMovieService.SearchMovieAsync(MovieCategory.now_playing,count),
                Popular = await _tmdbMovieService.SearchMovieAsync(MovieCategory.popular,count),
                TopRated = await _tmdbMovieService.SearchMovieAsync(MovieCategory.top_rated,count),
                Upcoming = await _tmdbMovieService.SearchMovieAsync(MovieCategory.upcoming,count)
            };
            return View(data);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
