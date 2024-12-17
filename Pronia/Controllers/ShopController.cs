using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.DAL;
using Pronia.Models;
using Pronia.Utilities.Enums;
using Pronia.Utilities.Exceptions;
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

        public async Task<IActionResult> Index(string? search, int? categoryId, int key = 1, int page = 1)
        {
            IQueryable<Product> query = _context.Products.Include(p => p.ProductImages.Where(pi => pi.IsPrimary != null));

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));
            }

            if (categoryId != null && categoryId > 0)
            {
                query = query.Where(pi => pi.CategoryId == categoryId);
            };

            switch (key)
            {
                case (int)ESortType.Name:
                    query = query.OrderBy(p => p.Name);
                    break;

                case (int)ESortType.Price:
                    query = query.OrderByDescending(p => p.Price);
                    break;

                case (int)ESortType.Date:
                    query = query.OrderBy(p => p.CreatedAt);
                    break;

                default:
                    break;
            }

            int count = query.Count();
            double totalPage = Math.Ceiling((double)count / 3);

            query = query.Skip((page - 1) * 3).Take(3);


            ShopVM shopVM = new ShopVM
            {
                Products = await query.Select(p => new GetProductVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Image = p.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true).ImageUrl,
                    SecondaryImage = p.ProductImages.FirstOrDefault(pi => pi.IsPrimary == false).ImageUrl,
                    Price = p.Price,
                }).ToListAsync(),

                Categories = await _context.Categories.Select(c => new GetCategoryVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    Count = c.Products.Count
                }).ToListAsync(),

                Search = search,
                CategoryId = categoryId,
                Key = key,
                TotalPage = totalPage,
                CurrentPage = page
            };



            return View(shopVM);
        }

        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null || id < 1) { throw new BadRequestException($"there is no product with {id} id"); };

            Product? product = await _context.Products
                .Include(p => p.ProductImages.OrderByDescending(pi => pi.IsPrimary)) //true,false,null ardcilligi
                .Include(p => p.Category)
                .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
                .Include(p => p.ProductColors).ThenInclude(pc => pc.Color)
                .Include(p => p.ProductSizes).ThenInclude(ps => ps.Size)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) { throw new NotFoundException($"couldn't find product with {id} id :( "); }

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
