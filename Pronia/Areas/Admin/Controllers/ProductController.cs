using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.Areas.Admin.ViewModels;
using Pronia.DAL;
using Pronia.Models;
using Pronia.Utilities.Enums;
using Pronia.Utilities.Extensions;

namespace Pronia.Areas.Admin.Controllers
{
    [Area("Admin")]
    //hem adminin hem moderatorun access-i var
    //[Authorize(Roles = "Admin,Moderator")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        //---------------------------

        public async Task<IActionResult> Index(int page = 1)
        {
            if (page < 1) return BadRequest();

            int count = await _context.Products.CountAsync();

            double total = Math.Ceiling((double)count / 5);

            if (page > total) return BadRequest();


            List<GetProductAdminVM> productsVMs = await _context.Products
                .Skip((page - 1) * 5)
                .Take(5)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Select(p => new GetProductAdminVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryName = p.Category.Name,
                    Image = p.ProductImages.FirstOrDefault(p => p.IsPrimary == true).ImageUrl

                }).ToListAsync();

            PaginatedVM<GetProductAdminVM> paginatedVM = new()
            {
                TotalPage = total,
                CurrentPage = page,
                Items = productsVMs
            };

            return View(paginatedVM);
        }

        //---------------------------

        public async Task<IActionResult> Create()
        {
            CreateProductVM productVM = new CreateProductVM
            {
                Categories = await _context.Categories.ToListAsync(),
                Tags = await _context.Tags.ToListAsync(),
                Colors = await _context.Colors.ToListAsync(),
                Sizes = await _context.Sizes.ToListAsync()

            };
            return View(productVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create(CreateProductVM productVM)
        {
            productVM.Categories = await _context.Categories.ToListAsync();
            productVM.Tags = await _context.Tags.ToListAsync();
            productVM.Colors = await _context.Colors.ToListAsync();
            productVM.Sizes = await _context.Sizes.ToListAsync();



            if (!ModelState.IsValid)
            {
                return View(productVM);
            }


            if (!productVM.MainPhoto.ValidateType("image/"))
            {
                ModelState.AddModelError(nameof(productVM.MainPhoto), "File format is not image");
                return View(productVM);
            }

            if (!productVM.MainPhoto.ValidateSize(FileSize.MB, 2))
            {
                ModelState.AddModelError(nameof(productVM.MainPhoto), "File size must be less than 2 MB");
                return View(productVM);
            }


            if (!productVM.HoverPhoto.ValidateType("image/"))
            {
                ModelState.AddModelError("HoverPhoto", "File fornat is not image");
                return View(productVM);
            }

            if (!productVM.HoverPhoto.ValidateSize(FileSize.MB, 2))
            {
                ModelState.AddModelError("HoverPhoto", "File size must be less than 2 MB");
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
                    ModelState.AddModelError(nameof(CreateProductVM.TagIds), "Tags are wrong");
                    return View();
                }
            }

            if (productVM.ColorIds is not null)
            {
                bool colorResult = productVM.ColorIds.Any(cId => !productVM.Colors.Exists(c => c.Id == cId));

                if (colorResult)
                {
                    ModelState.AddModelError(nameof(CreateProductVM.ColorIds), "Colors are wrong");
                    return View();
                }
            }

            if (productVM.SizeIds is not null)
            {
                bool sizeResult = productVM.SizeIds.Any(sId => !productVM.Sizes.Exists(s => s.Id == sId));

                if (sizeResult)
                {
                    ModelState.AddModelError(nameof(CreateProductVM.SizeIds), "Sizes are wrong");
                    return View();
                }
            }


            ProductImage mainImage = new()
            {
                ImageUrl = await productVM.MainPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images"),
                IsPrimary = true,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };

            ProductImage hoverImage = new()
            {
                ImageUrl = await productVM.HoverPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images"),
                IsPrimary = false,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };

            Product product = new()
            {
                Name = productVM.Name,
                SKU = productVM.SKU,
                CategoryId = productVM.CategoryId.Value,
                Description = productVM.Description,
                Price = productVM.Price.Value,
                CreatedAt = DateTime.Now,
                IsDeleted = false,
                ProductImages = new List<ProductImage>
                {
                    mainImage, hoverImage
                }
            };


            if (productVM.TagIds is not null)
            {
                product.ProductTags = productVM.TagIds.Select(tId => new ProductTag { TagId = tId }).ToList();
            }

            if (productVM.ColorIds is not null)
            {
                product.ProductColors = productVM.ColorIds.Select(cId => new ProductColor { ColorId = cId }).ToList();
            }

            if (productVM.SizeIds is not null)
            {
                product.ProductSizes = productVM.SizeIds.Select(sId => new ProductSize { SizeId = sId }).ToList();
            }

            if (productVM.AdditionalPhotos is not null)
            {
                string text = string.Empty;

                foreach (IFormFile file in productVM.AdditionalPhotos)
                {

                    if (!file.ValidateType("image/"))
                    {
                        text +=
                            $"<p class=\"text-danger\">Type of {file.FileName} must be image!</p>";
                        continue;
                    }

                    if (!file.ValidateSize(FileSize.MB, 2))
                    {
                        text += $"<p class=\"text-danger\">Size of {file.FileName} must be less than 2 MB!</p>";
                        continue;
                    }

                    product.ProductImages.Add(new ProductImage
                    {
                        ImageUrl = await file.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images"),
                        CreatedAt = DateTime.Now,
                        IsDeleted = false,
                        IsPrimary = null
                    });
                }
                TempData["FileWarning"] = text;
            }

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        //---------------------------
        //ancaq adminin access-i var

        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id < 1) { return BadRequest(); }

            Product product = await _context.Products
                .Include(p => p.ProductTags)
                .Include(p => p.ProductColors)
                .Include(p => p.ProductSizes)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) { return NotFound(); }

            UpdateProductVM productVM = new()
            {
                Name = product.Name,
                SKU = product.SKU,
                CategoryId = product.CategoryId,
                Description = product.Description,
                Price = product.Price,
                ProductImages = product.ProductImages,

                TagIds = product.ProductTags.Select(pt => pt.TagId).ToList(),
                ColorIds = product.ProductColors.Select(pc => pc.ColorId).ToList(),
                SizeIds = product.ProductSizes.Select(ps => ps.SizeId).ToList(),

                Categories = await _context.Categories.ToListAsync(),
                Tags = await _context.Tags.ToListAsync(),
                Colors = await _context.Colors.ToListAsync(),
                Sizes = await _context.Sizes.ToListAsync()
            };

            return View(productVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Update(int? id, UpdateProductVM productVM)
        {
            if (id == null || id < 1) { return BadRequest(); }

            Product existed = await _context.Products
                .Include(p => p.ProductTags)
                .Include(p => p.ProductColors)
                .Include(p => p.ProductSizes)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existed == null) { return NotFound(); }


            productVM.Categories = await _context.Categories.ToListAsync();
            productVM.Tags = await _context.Tags.ToListAsync();
            productVM.Colors = await _context.Colors.ToListAsync();
            productVM.Sizes = await _context.Sizes.ToListAsync();
            productVM.ProductImages = existed.ProductImages;



            if (!ModelState.IsValid)
            {
                return View(productVM);
            }

            if (productVM.MainPhoto is not null)
            {
                if (!productVM.MainPhoto.ValidateType("image/"))
                {
                    ModelState.AddModelError(nameof(productVM.MainPhoto), "File format is not image");
                    return View(productVM);
                }

                if (!productVM.MainPhoto.ValidateSize(FileSize.MB, 2))
                {
                    ModelState.AddModelError(nameof(productVM.MainPhoto), "File size must be less than 2 MB");
                    return View(productVM);
                }
            }

            if (productVM.HoverPhoto is not null)
            {
                if (!productVM.MainPhoto.ValidateType("image/"))
                {
                    ModelState.AddModelError(nameof(productVM.HoverPhoto), "File format is not image");
                    return View(productVM);
                }

                if (!productVM.MainPhoto.ValidateSize(FileSize.MB, 2))
                {
                    ModelState.AddModelError(nameof(productVM.HoverPhoto), "File size must be less than 2 MB");
                    return View(productVM);
                }
            }



            if (existed.CategoryId != productVM.CategoryId)
            {
                bool result = productVM.Categories.Any(c => c.Id == productVM.CategoryId);
                if (!result)
                {
                    return View(productVM);
                }
            }


            #region Tag

            if (productVM.TagIds is not null)
            {
                bool tagResult = productVM.TagIds.Any(tId => !productVM.Tags.Exists(t => t.Id == tId));

                if (tagResult)
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.TagIds), "Tags are wrong");
                    return View();
                }
            }


            if (productVM.TagIds is null)
            {
                productVM.TagIds = new();
            }
            else
            {
                productVM.TagIds = productVM.TagIds.Distinct().ToList();
            }


            _context.ProductTags.RemoveRange(existed.ProductTags.Where(pTag => !productVM.TagIds.Exists(tId => tId == pTag.TagId)).ToList());


            _context.ProductTags.AddRange(productVM.TagIds
            .Where(tId => !existed.ProductTags.Exists(pTag => pTag.TagId == tId))
            .ToList()
            .Select(tId => new ProductTag { TagId = tId, ProductId = existed.Id }));

            #endregion

            if (productVM.ColorIds is not null)
            {
                bool colorResult = productVM.ColorIds.Any(cId => !productVM.Colors.Exists(c => c.Id == cId));

                if (colorResult)
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.ColorIds), "Colors are wrong");
                    return View();
                }
            }

            if (productVM.ColorIds is null)
            {
                productVM.ColorIds = new();
            }
            else
            {
                productVM.ColorIds = productVM.ColorIds.Distinct().ToList();
            }
            _context.ProductColors.RemoveRange(existed.ProductColors.Where(pColor => !productVM.ColorIds.Exists(cId => cId == pColor.ColorId)).ToList());

            _context.ProductColors.AddRange(productVM.ColorIds
            .Where(cId => !existed.ProductColors.Exists(pColor => pColor.ColorId == cId))
            .ToList()
            .Select(cId => new ProductColor { ColorId = cId, ProductId = existed.Id }));




            if (productVM.SizeIds is not null)
            {
                bool sizeResult = productVM.SizeIds.Any(sId => !productVM.Sizes.Exists(s => s.Id == sId));

                if (sizeResult)
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.SizeIds), "Sizes are wrong");
                    return View();
                }
            }


            if (productVM.SizeIds is null)
            {
                productVM.SizeIds = new();
            }
            else
            {
                productVM.SizeIds = productVM.SizeIds.Distinct().ToList();
            }
            _context.ProductSizes.RemoveRange(existed.ProductSizes.Where(pSize => !productVM.SizeIds.Exists(sId => sId == pSize.SizeId)).ToList());

            _context.ProductSizes.AddRange(productVM.SizeIds
            .Where(sId => !existed.ProductSizes.Exists(pSize => pSize.SizeId == sId))
            .ToList()
            .Select(sId => new ProductSize { SizeId = sId, ProductId = existed.Id }));




            if (productVM.MainPhoto is not null)
            {
                string fileName = await productVM.MainPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images");

                ProductImage main = existed.ProductImages.FirstOrDefault(p => p.IsPrimary == true);
                main.ImageUrl.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
                existed.ProductImages.Remove(main);
                existed.ProductImages.Add(new ProductImage
                {
                    CreatedAt = DateTime.Now,
                    IsDeleted = false,
                    IsPrimary = true,
                    ImageUrl = fileName
                });
            }

            if (productVM.HoverPhoto is not null)
            {
                string fileName = await productVM.HoverPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images");

                ProductImage hover = existed.ProductImages.FirstOrDefault(p => p.IsPrimary == false);
                hover.ImageUrl.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
                existed.ProductImages.Remove(hover);
                existed.ProductImages.Add(new ProductImage
                {
                    CreatedAt = DateTime.Now,
                    IsDeleted = false,
                    IsPrimary = false,
                    ImageUrl = fileName
                });
            }


            if (productVM.ImageIds is not null)
            {
                productVM.ImageIds = new List<int>();
            }
            var deletedImages = existed.ProductImages.Where(pi => !productVM.ImageIds.Exists(imgId => imgId == pi.Id) && pi.IsPrimary == null).ToList();

            deletedImages.ForEach(di => di.ImageUrl.DeleteFile(_env.WebRootPath, "assets", "images", "website-images"));

            _context.ProductImages.RemoveRange(deletedImages);

            if (productVM.AdditionalPhotos is not null)
            {
                string text = string.Empty;

                foreach (IFormFile file in productVM.AdditionalPhotos)
                {
                    if (!file.ValidateType("image/"))
                    {
                        text +=
                            $"<p class=\"text-danger\">Type of {file.FileName} must be image!</p>";
                        continue;
                    }

                    if (!file.ValidateSize(FileSize.MB, 2))
                    {
                        text += $"<p class=\"text-danger\">Size of {file.FileName} must be less than 2 MB!</p>";
                        continue;
                    }

                    existed.ProductImages.Add(new ProductImage
                    {
                        ImageUrl = await file.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images"),
                        CreatedAt = DateTime.Now,
                        IsDeleted = false,
                        IsPrimary = null
                    });
                }
                TempData["FileWarning"] = text;

            }



            existed.SKU = productVM.SKU;
            existed.Price = productVM.Price.Value;
            existed.CategoryId = productVM.CategoryId.Value;
            existed.Description = productVM.Description;
            existed.Name = productVM.Name;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }

        //---------------------------

        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id < 1) return BadRequest();

            Product product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductTags)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null) return NotFound();

            foreach (var image in product.ProductImages)
            {
                image.ImageUrl.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
            }

            _context.ProductTags.RemoveRange(product.ProductTags);

            _context.ProductImages.RemoveRange(product.ProductImages);

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}
