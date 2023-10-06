using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MovieProDemo.Data;
using MovieProDemo.Models.Database;
using MovieProDemo.Models.Settings;
using MovieProDemo.Models.TMDB;
using MovieProDemo.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieProDemo.Controllers
{
    public class MoviesController : Controller
    {
        private readonly AppSettings _appSettings;
        private readonly ApplicationDbContext _db;
        private readonly IImageService _imageService;
        private readonly IRemoteMovieService _tmdbMovieService;
        private readonly IDataMappingService _tmdbMappingService;
        public MoviesController(IOptions<AppSettings> appSettings,ApplicationDbContext db,IImageService imgService,IRemoteMovieService tmdbMovieService,IDataMappingService tmdbMappingService)
        {
            _appSettings = appSettings.Value;
            _db = db;
            _imageService = imgService;
            _tmdbMovieService = tmdbMovieService;
            _tmdbMappingService = tmdbMappingService;
        }
        public async Task<IActionResult> Import()
        {
            var movies = await _db.Movie.ToListAsync();

            return View(movies);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(int id)
        {
            //If we already have this movie we can just warn the user instead of importing it again
            if (_db.Movie.Any(m => m.Id == id))
            {
                var localMovie = await _db.Movie.FirstOrDefaultAsync(m => m.MovieId == id);
                return RedirectToAction("Details","Movies",new { id = localMovie.Id,local = true});
            }

            //Step 1: Get the raw data from the API
            var movieDetail = await _tmdbMovieService.MovieDetailAsync(id);

            //Step 2: Run the data through a mapping procedure
            var movie = await _tmdbMappingService.MapMovieDetailAsync(movieDetail);

            //Step 3: Add the new movie
            _db.Add(movie);
            await _db.SaveChangesAsync();

            //Step 4: Assign it to the default all collection
            await AddToMovieCollection(movie.Id,_appSettings.MovieProSettings.DefaultCollection.Name);

            return RedirectToAction("Import");
        }
        public async Task<IActionResult> Library()
        {
            var movies = await _db.Movie.ToListAsync();

            return View(movies);
        }
        // GET: Movie/Create
        public IActionResult Create()
        {
            ViewData["CollectionId"] = new SelectList(_db.Collection,"Id","Name");
            return View();
        }

        // POST: Movie/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MovieId,Title,TagLine,Overview,RunTime,ReleaseDate,Rating,VoteAverage,Poster,PosterType,Backdrop,BackdropType,TrailerUrl")] Movie movie, int collectionId)
        {
            if (ModelState.IsValid)
            {
                movie.PosterType = movie.PosterFile?.ContentType;
                movie.Poster = await _imageService.EncodeImageAsync(movie.PosterFile);

                movie.BackdropType = movie.BackdropFile?.ContentType;
                movie.Backdrop = await _imageService.EncodeImageAsync(movie.BackdropFile);

                _db.Add(movie);
                await _db.SaveChangesAsync();

                await AddToMovieCollection(movie.Id,collectionId);

                return RedirectToAction("Index","MovieCollections");
            }
            return View(movie);
        }

        // GET: Movie/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _db.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movie/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MovieId,Title,TagLine,Overview,RunTime,ReleaseDate,Rating,VoteAverage,Poster,PosterType,Backdrop,BackdropType,TrailerUrl")] Movie movie)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (movie.PosterFile is not null)
                    {
                        movie.PosterType = movie.PosterFile?.ContentType;
                        movie.Poster = await _imageService.EncodeImageAsync(movie.PosterFile);
                    }

                    if (movie.BackdropFile is not null)
                    {
                        movie.BackdropType = movie.BackdropFile?.ContentType;
                        movie.Backdrop = await _imageService.EncodeImageAsync(movie.BackdropFile);
                    }
                    //Add the movie to the db<set>
                    _db.Update(movie);

                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details","Movies", new { id = movie.Id, local = true});
            }
            return View(movie);
        }

        // GET: Movie/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _db.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movie/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _db.Movie.FindAsync(id);
            _db.Movie.Remove(movie);
            await _db.SaveChangesAsync();
            return RedirectToAction("Library","Movies");
        }
        public async Task<IActionResult> Details(int? id, bool isLocal = false)
        {
     
            if (id == null)
            {
                return NotFound();
            }

            Movie movie = new();

            if (isLocal == true)
            {
                //Get the data from the database
                movie = await _db.Movie.Include(m => m.Cast).Include(m => m.Crew).FirstOrDefaultAsync(m => m.Id == id);
            }
            else
            {
                //Get the data from the TMDB API
                MovieDetail movieDetail = await _tmdbMovieService.MovieDetailAsync((int)id);
                movie = await _tmdbMappingService.MapMovieDetailAsync(movieDetail);
            }

            if (movie == null)
            {
                return NotFound();
            }
            ViewData["Local"] = isLocal;
            return View(movie);
        }
        private bool MovieExists(int id)
        {
            return _db.Movie.Any(e => e.Id == id);
        }

        private async Task AddToMovieCollection(int movieId, string collectionName)
        {
            var collection = await _db.Collection.FirstOrDefaultAsync(c => c.Name == collectionName);
            _db.Add(
                new MovieCollection()
                {
                    CollectionId = collection.Id,
                    MovieId = movieId
                }
             );
            await _db.SaveChangesAsync();
        }
        private async Task AddToMovieCollection(int movieId,int collectionId)
        {
            _db.Add(
                new MovieCollection()
                {
                    CollectionId = collectionId,
                    MovieId = movieId
                }
            );

            await _db.SaveChangesAsync();
        }


    }
}
