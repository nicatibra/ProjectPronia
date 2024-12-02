using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.DAL;
using Pronia.Models;
using Pronia.ViewModels;

namespace Pronia.Controllers
{
    public class ShopController : Controller
    {
        private readonly AppDbContext _context;

        public ShopController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null || id < 1) { return BadRequest(); }

            Product? product = await _context.Products
                .Include(p => p.ProductImages.OrderByDescending(pi => pi.IsPrimary)) //true,false,null ardcilligi
                .Include(p => p.Category)
                .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
                .Include(p => p.ProductColors).ThenInclude(pc => pc.Color)
                .Include(p => p.ProductSizes).ThenInclude(ps => ps.Size)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) { return NotFound(); }

            DetailVM detailVM = new DetailVM
            {
                Product = product,

                RelatedProducts = await _context.Products
                .Take(8)
                .Where(p => p.CategoryId == product.CategoryId && p.Id != id)
                .Include(p => p.ProductImages.Where(pi => pi.IsPrimary != null))
                .ToListAsync()
            };

            return View(detailVM);
        }
    }
}
