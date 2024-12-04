using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.Areas.Admin.ViewModels.Colors;
using Pronia.DAL;
using Pronia.Models;

namespace Pronia.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Moderator")]

    public class ColorController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ColorController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            List<Color> colors = await _context.Colors.ToListAsync();
            return View(colors);
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateColorVM colorVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            bool result = await _context.Colors.AnyAsync(c => c.Name.Trim() == colorVM.Name.Trim());

            if (result)
            {
                ModelState.AddModelError(nameof(colorVM.Name), $"{colorVM.Name} is already exist");
                return View();
            }

            Models.Color color = new()
            {
                Name = colorVM.Name
            };

            await _context.Colors.AddAsync(color);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null || id < 1) return BadRequest();
            Color color = await _context.Colors.FirstOrDefaultAsync(c => c.Id == id);
            if (color is null) return NotFound();

            _context.Colors.Remove(color);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id is null || id < 1) return BadRequest();
            Color color = await _context.Colors.FirstOrDefaultAsync(c => c.Id == id);
            if (color is null) return NotFound();

            UpdateColorVM colorVM = new UpdateColorVM()
            {
                Name = color.Name
            };

            return View(colorVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateColorVM colorVM)
        {
            if (!ModelState.IsValid)
            {
                return View(colorVM);
            }

            Color existed = await _context.Colors.FirstOrDefaultAsync(c => c.Id == id);
            if (existed is null) return NotFound();

            existed.Name = colorVM.Name;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
