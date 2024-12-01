using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.Areas.Admin.ViewModels;
using Pronia.DAL;
using Pronia.Models;



namespace Pronia.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            List<Category> categories = await _context.Categories
                .Where(c => !c.IsDeleted) //false olanlari getirir
                .Include(c => c.Products)
                .ToListAsync();

            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryVM categoryVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            bool result = await _context.Categories.AnyAsync(c => c.Name.Trim() == categoryVM.Name.Trim()); //Any() avtomatik ToLower edir

            if (result)
            {
                ModelState.AddModelError("Name", "Category already exists!");
                return View();
            }

            Category category = new()
            {
                Name = categoryVM.Name,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };


            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id < 1) { return BadRequest(); }

            Category category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) { return NotFound(); }

            UpdateCategoryVM categoryVM = new UpdateCategoryVM
            {
                Name = category.Name
            };

            return View(categoryVM);
        }


        //existed: SQl-den gelen       category:formda daxil edilen
        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateCategoryVM categoryVM)
        {
            if (id == null || id < 1) { return BadRequest(); }

            Category existed = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

            if (categoryVM == null) { return NotFound(); }

            if (!ModelState.IsValid)
            {
                return View(categoryVM);
            }

            bool result = await _context.Categories.AnyAsync(c => c.Name.Trim() == categoryVM.Name.Trim() && c.Id != id);
            if (result)
            {
                ModelState.AddModelError(nameof(UpdateCategoryVM.Name), "Category adlready exists!");
                return View(categoryVM);
            }

            if (existed.Name == categoryVM.Name)
            {
                return RedirectToAction(nameof(Index));
            }


            existed.Name = categoryVM.Name;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id < 1) { return BadRequest(); }

            Category category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) { return NotFound(); }

            category.IsDeleted = true;
            await _context.SaveChangesAsync();

            #region PermanentDelete
            //_context.Categories.Remove(category);
            //await _context.SaveChangesAsync(); 
            #endregion

            return RedirectToAction(nameof(Index));
        }
    }
}
