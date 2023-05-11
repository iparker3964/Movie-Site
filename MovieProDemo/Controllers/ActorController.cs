using Microsoft.AspNetCore.Mvc;
using MovieProDemo.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MovieProDemo.Services.Interfaces;

namespace MovieProDemo.Controllers
{
    public class ActorController : Controller
    {
        private readonly IDataMappingService _tmdbMappingService;
        private readonly IRemoteMovieService _tmdbMovieService;
        public ActorController(IRemoteMovieService tmdbMovieService, IDataMappingService tmdbMappingService)
        {
            _tmdbMappingService = tmdbMappingService;
            _tmdbMovieService = tmdbMovieService;
        }
        public async Task<IActionResult> Details(int id)
        {
            var actor = await _tmdbMovieService.ActorDetailAsync(id);
            actor = _tmdbMappingService.MapActorDetail(actor);
            return View(actor);
        }
    }
}
