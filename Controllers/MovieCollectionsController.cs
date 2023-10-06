using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MovieProDemo.Data;
using MovieProDemo.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieProDemo.Controllers
{
    public class MovieCollectionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MovieCollectionsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int? id)
        {
            id ??= (await _context.Collection.FirstOrDefaultAsync(c => c.Name.ToUpper().Equals("ALL"))).Id;

            ViewData["CollectionId"] = new SelectList(_context.Collection,"Id","Name",id);

            var allMovieIds = await _context.Movie.Select(m => m.Id).ToListAsync();

            var movieIdsInCollection = await _context.MovieCollection.Where(m => m.CollectionId == id).OrderBy(m => m.Order).Select(m => m.MovieId).ToListAsync();
            
            var movieIdsNotInCollection = allMovieIds.Except(movieIdsInCollection);

            List<Movie> moviesInCollection = new List<Movie>();

            movieIdsInCollection.ForEach(m => moviesInCollection.Add(_context.Movie.Find(m)));
            //var moviesInCollection = await _context.Movie.Where(m => movieIdsInCollection.Contains(m.Id)).ToListAsync();

            ViewData["IdsInCollection"] = new MultiSelectList(moviesInCollection,"Id","Title");

            var moviesNotInCollection = _context.Movie.AsNoTracking().Where(m => movieIdsNotInCollection.Contains(m.Id));
           
            ViewData["IdsNotInCollection"] = new MultiSelectList(moviesNotInCollection, "Id","Title");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(int id, List<int> idsInCollection)
        {
            //Remove old movies
            var oldRecords = _context.MovieCollection.Where(mc => mc.CollectionId == id);
            _context.MovieCollection.RemoveRange(oldRecords);

            await _context.SaveChangesAsync();

            //Add new movie collection
            int orderCounter = 1;
            idsInCollection.ForEach(movieId =>
            {
                _context.Add(new MovieCollection
                {
                    CollectionId = id,
                    MovieId = movieId,
                    Order = orderCounter++
                });
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index),new {id});
        }
    }
}
