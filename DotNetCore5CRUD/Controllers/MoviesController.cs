using DotNetCore5CRUD.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DotNetCore5CRUD.ViewModels;
using System.IO;
using NToastNotify;

namespace DotNetCore5CRUD.Controllers
{
    public class MoviesController : Controller
    {

        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var movies = _context.Movies.OrderByDescending(m => m.Rate).ToList();
            return View(movies);
        }
        public IActionResult Create()
        {
            var viewmodel = new MovieFormViewModel {
                Genres = _context.Genres.OrderBy(m => m.Name).ToList()
            };
            return View("MovieForm", viewmodel);
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult Create(MovieFormViewModel movie_view_model)
        {
            if(!ModelState.IsValid)
            {
                movie_view_model.Genres = _context.Genres.OrderBy(m => m.Name).ToList();
                return View("MovieForm",movie_view_model);
            }

            var files = Request.Form.Files;
            if(!files.Any())
            {
                movie_view_model.Genres = _context.Genres.OrderBy(m => m.Name).ToList();
                ModelState.AddModelError("Poster", "Please Select Movie Poster");
                return View("MovieForm", movie_view_model);
            }

            var poster = files.FirstOrDefault();
            List<string> Allowed_extention = new List<string> { ".jpg", ".png" };
            if(!Allowed_extention.Contains(Path.GetExtension(poster.FileName).ToLower()))
            {
                movie_view_model.Genres = _context.Genres.OrderBy(m => m.Name).ToList();
                ModelState.AddModelError("Poster", "Only .PNG ,.JPG images are allowed!");
                return View("MovieForm", movie_view_model);

            }

            if(poster.Length>1048576)
            {
                movie_view_model.Genres = _context.Genres.OrderBy(m => m.Name).ToList();
                ModelState.AddModelError("Poster", "Poster cann't be more than 1MB!");
                return View("MovieForm", movie_view_model);

            }

            var filestream = new MemoryStream();
            poster.CopyTo(filestream);
            var movie = new Movie
            {
                Title = movie_view_model.Title,
                Rate = movie_view_model.Rate,
                Year = movie_view_model.Year,
                Storeline = movie_view_model.Storeline,
                GenreId = movie_view_model.GenreId,
                Poster = filestream.ToArray()
            };
            _context.Movies.Add(movie);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));

        }
       public IActionResult Edit(int ?id)
        {
            if (id == null)
                return BadRequest();
            var movie = _context.Movies.Find(id);
            if (movie == null)
                return NotFound();
            var viewDTO = new MovieFormViewModel
            {
                Id = movie.Id,
                Year = movie.Year,
                Rate = movie.Rate,
                Title = movie.Title,
                Storeline = movie.Storeline,
                Poster = movie.Poster,
                GenreId = movie.GenreId,
                Genres = _context.Genres.OrderBy(m => m.Name).ToList()

            };
            return View("MovieForm",viewDTO);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(MovieFormViewModel model)
        {
            if(!ModelState.IsValid)
            {
                model.Genres = _context.Genres.OrderBy(m => m.Name).ToList();
                return View("MovieForm", model);
             }

           var movie = _context.Movies.Find(model.Id);
            movie.Title = model.Title;
            movie.Year = model.Year;
            movie.Rate = model.Rate;
            movie.Storeline = model.Storeline;
            movie.GenreId = model.GenreId;
            //_context.Movies.Update(movie);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Details(int?id)
        {
            if (id == null)
                return BadRequest();
            var movie = _context.Movies.Include(m => m.Genre).SingleOrDefault(m => m.Id == id);
            if (movie == null)
                return NotFound();

            return View(movie);
        }
        public IActionResult Delete(int?id)
        {
            if (id == null)
                return BadRequest();
            var movie = _context.Movies.Find(id);
            if (movie == null)
                return NotFound();
            _context.Movies.Remove(movie);
            int i=_context.SaveChanges();
            // return RedirectToAction(nameof(Index)); da lw ana 32wz anfz el function bt23t el Delete zy el (Edit,Create,Details)
            return Ok();// de el t2reqa bt23t el Ajax => we de bt5lini a delete el film bdwoon ma a load el sf7a aw a3ml redirectToAction()
        }
    }
}
