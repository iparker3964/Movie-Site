using Microsoft.Extensions.Options;
using MovieProDemo.Enums;
using MovieProDemo.Models.Database;
using MovieProDemo.Models.Settings;
using MovieProDemo.Models.TMDB;
using MovieProDemo.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MovieProDemo.Services
{
    public class TMDBMappingService : IDataMappingService
    {
        private readonly AppSettings _appSettings;
        private readonly IImageService _imageService;
        public TMDBMappingService(IOptions<AppSettings> appSettings, IImageService imageService)
        {
            _appSettings = appSettings.Value;
            _imageService = imageService;
        }
        public ActorDetail MapActorDetail(ActorDetail actor)
        {
            actor.profile_path = BuildCastImage(actor.profile_path);

            if (string.IsNullOrEmpty(actor.biography))
            {
                actor.biography = "Not Available";
            }
            if (string.IsNullOrEmpty(actor.place_of_birth))
            {
                actor.place_of_birth = "Not Available";
            }
            if (string.IsNullOrEmpty(actor.birthday))
            {
                actor.birthday = "Not Available";
            }
            else
            {
                actor.birthday = DateTime.Parse(actor.birthday).ToString("MMM dd, yyyy");
            }

            return actor;
        }

        public async Task<Movie> MapMovieDetailAsync(MovieDetail movie)
        {
            Movie newMovie = null;

            try
            {
                newMovie = new Movie()
                {
                    MovieId = movie.id,
                    Title = movie.title,
                    TagLine = movie.tagline,
                    Overview = movie.overview,
                    RunTime = movie.runtime,
                    VoteAverage = movie.vote_average,
                    ReleaseDate = DateTime.Parse(movie.release_date),
                    TrailerUrl = BuildTrailerPath(movie.videos),
                    Backdrop = await EncodeBackDropImageAsync(movie.backdrop_path),
                    BackdropType = BuildImageType(movie.backdrop_path),
                    Poster = await EncodePosterImageAsync(movie.poster_path),
                    PosterType = BuildImageType(movie.poster_path),
                    Rating = GetRating(movie.release_dates)
                };

                var castMembers = movie.credits.cast.OrderByDescending(c => c.popularity).GroupBy(c => c.cast_id).Select(c => c.FirstOrDefault()).Take(20).ToList();

                castMembers.ForEach(x =>
                {
                    newMovie.Cast.Add(new MovieCast()
                    {
                        CastId = x.id,
                        Department = x.known_for_department,
                        Name = x.name,
                        Character = x.character,
                        ImageUrl = BuildCastImage(x.profile_path),
                    });
                });

                var crewMembers = movie.credits.crew.OrderByDescending(x => x.popularity).GroupBy(c => c.id).Select(g => g.First()).Take(20).ToList();

                crewMembers.ForEach(x =>
                {
                    newMovie.Crew.Add(new MovieCrew()
                    {
                        CrewId = x.id,
                        Department = x.department,
                        Name = x.name,
                        Job = x.job,
                        ImageUrl = BuildCastImage(x.profile_path)
                    });
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Map Movie Detail Async: {ex.Message}");
            }
            return newMovie;
        }
private string BuildCastImage(string profilePath)
{
    if (string.IsNullOrEmpty(profilePath))
    {
        return _appSettings.MovieProSettings.DefaultCastImage;
    }

    return $"{_appSettings.TMDBSettings.BaseImagePath}/{_appSettings.MovieProSettings.DefaultPosterSize}/{profilePath}";
}
private MovieRating GetRating(Release_Dates dates)
{
    var movieRating = MovieRating.NR;
    var certification = dates.results.FirstOrDefault(x => x.iso_3166_1.Equals("US"));

    if (certification is not null)
    {
        var apiRating = certification.release_dates.FirstOrDefault(x => x.certification != "")?.certification.Replace("-", "");
        if (!string.IsNullOrEmpty(apiRating))
        {
            movieRating = (MovieRating)Enum.Parse(typeof(MovieRating), apiRating, true);
        }
    }
    return movieRating;
}
private string BuildTrailerPath(Videos videos)
{
    var videoKey = videos.results.FirstOrDefault(x => x.type.ToLower().Trim().Equals("trailer") && !x.key.Equals(""))?.key;
    return string.IsNullOrEmpty(videoKey) ? videoKey : $"{_appSettings.TMDBSettings.BaseYouTubePath}{videoKey}";
}
private async Task<byte[]> EncodeBackDropImageAsync(string path)
{
    var backDropPath = $"{_appSettings.TMDBSettings.BaseImagePath}/{_appSettings.MovieProSettings.DefaultBackdropSize}/{path}";
    return await _imageService.EncodeImageURLAsync(backDropPath);
}
private string BuildImageType(string path)
{
    if (string.IsNullOrEmpty(path))
    {
        return path;
    }

    return $"image/{Path.GetExtension(path).TrimStart('.')}";
}
private async Task<byte[]> EncodePosterImageAsync(string path)
{
    var posterPath = $"{_appSettings.TMDBSettings.BaseImagePath}/{_appSettings.MovieProSettings.DefaultPosterSize}/{path}";
    return await _imageService.EncodeImageURLAsync(posterPath);
}
    }
}
