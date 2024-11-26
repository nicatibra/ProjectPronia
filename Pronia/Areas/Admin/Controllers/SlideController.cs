using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.DAL;
using Pronia.Models;
using Pronia.Utilities.Extensions;

namespace Pronia.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SlideController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SlideController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            List<Slide> slides = await _context.Slides.ToListAsync();
            return View(slides);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Slide slide)
        {
            //if (!ModelState.IsValid) { return View(); };

            if (!slide.Photo.ValidateType("image/"))
            {
                ModelState.AddModelError("Photo", "File type must be image!");
                return View();
            }
            if (!slide.Photo.ValidateSize(Utilities.Enums.FileSize.MB, 5))
            {
                ModelState.AddModelError("Photo", "File size must be less than 5 mb");
                return View();
            }

            slide.ImageUrl = await slide.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images");

            await _context.Slides.AddAsync(slide);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));


        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id < 1) { return BadRequest(); }

            Slide slide = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);

            if (slide == null) { return NotFound(); }

            slide.ImageUrl.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");


            _context.Slides.Remove(slide);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
