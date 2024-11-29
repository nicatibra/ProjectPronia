using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.Areas.Admin.ViewModels;
using Pronia.DAL;
using Pronia.Models;

namespace Pronia.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            List<GetProductAdminVM> productsVMs = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages.Where(pi => pi.IsPrimary == true))
                .Select(p => new GetProductAdminVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryName = p.Category.Name,
                    Image = p.ProductImages[0].ImageUrl
                })
                .ToListAsync();
            return View(productsVMs);
        }

        public async Task<IActionResult> Create()
        {
            CreateProductVM productVM = new CreateProductVM
            {
                Tags = await _context.Tags.ToListAsync(),
                Categories = await _context.Categories.ToListAsync()
            };
            return View(productVM);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM productVM)
        {
            productVM.Categories = await _context.Categories.ToListAsync();
            productVM.Tags = await _context.Tags.ToListAsync();


            if (!ModelState.IsValid)
            {
                return View(productVM);
            }

            bool result = productVM.Categories.Any(c => c.Id == productVM.CategoryId);
            if (!result)
            {
                ModelState.AddModelError(nameof(CreateProductVM.CategoryId), "Category does not exist");
                return View(productVM);
            }

            if (productVM.TagIds is not null)
            {
                bool tagResult = productVM.TagIds.Any(tId => !productVM.Tags.Exists(t => t.Id == tId));

                if (tagResult)
                {
                    ModelState.AddModelError(nameof(CreateProductVM.TagIds), "Tags are worng");
                    return View();
                }
            }



            Product product = new()
            {
                Name = productVM.Name,
                SKU = productVM.SKU,
                CategoryId = productVM.CategoryId.Value,
                Description = productVM.Description,
                Price = productVM.Price.Value,
                CreatedAt = DateTime.Now,
                IsDeleted = false,
            };

            if (productVM.TagIds is not null)
            {
                product.ProductTags = productVM.TagIds.Select(tId => new ProductTag { TagId = tId }).ToList();

            }

            //Yuxarida daha qisa yazlis
            //foreach (var tId in productVM.TagIds)
            //{
            //    product.ProductTags.Add(
            //    new ProductTag
            //    {
            //        TagId = tId,
            //        Product = product
            //    });
            //}

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id < 1) { return BadRequest(); }

            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) { return NotFound(); }

            UpdateProductAdminVM productAdminVM = new()
            {
                Name = product.Name,
                SKU = product.SKU,
                CategoryId = product.CategoryId,
                Description = product.Description,
                Price = product.Price,
                TagIds = product.ProductTags.Select(pt => pt.TagId).ToList(),
                Categories = await _context.Categories.ToListAsync(),
                Tags = await _context.Tags.ToListAsync()
            };

            return View(productAdminVM);
        }

        [HttpPost]

        public async Task<IActionResult> Update(int? id, UpdateProductAdminVM productAdminVM)
        {
            if (id == null || id < 1) { return BadRequest(); }

            productAdminVM.Categories = await _context.Categories.ToListAsync();

            if (!ModelState.IsValid)
            {
                return View(productAdminVM);
            }

            Product existed = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (existed == null) { return NotFound(); }

            if (existed.CategoryId != productAdminVM.CategoryId)
            {
                bool result = productAdminVM.Categories.Any(c => c.Id == productAdminVM.CategoryId);
                if (!result)
                {
                    return View(productAdminVM);
                }
            }

            existed.SKU = productAdminVM.SKU;
            existed.Price = productAdminVM.Price.Value;
            existed.CategoryId = productAdminVM.CategoryId.Value;
            existed.Description = productAdminVM.Description;
            existed.Name = productAdminVM.Name;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }
    }
}
