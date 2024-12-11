using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.DAL;
using Pronia.Models;
using Pronia.Utilities.Enums;

namespace Pronia.ViewComponents
{
    public class ProductViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public ProductViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(ESortType type)
        {
            List<Product> products = null;

            switch (type)
            {
                case ESortType.Name:
                    products = await _context.Products
                        .OrderBy(p => p.Name)
                        .Take(8)
                        .Include(p => p.ProductImages
                        .Where(pi => pi.IsPrimary != null))
                        .ToListAsync();
                    break;

                case ESortType.Price:
                    products = await _context.Products
                        .OrderByDescending(p => p.Price)
                        .Take(8)
                        .Include(p => p.ProductImages
                        .Where(pi => pi.IsPrimary != null))
                        .ToListAsync();

                    break;
                case ESortType.Date:
                    products = await _context.Products
                        .OrderByDescending(p => p.CreatedAt)
                        .Take(8)
                        .Include(p => p.ProductImages
                        .Where(pi => pi.IsPrimary != null))
                        .ToListAsync();
                    break;
                default:
                    break;
            }

            return View(products);
        }
    }
}
