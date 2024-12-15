using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pronia.DAL;
using Pronia.Models;
using Pronia.ViewModels;
using Pronia.ViewModels.Basket;
using System.Security.Claims;

namespace Pronia.Controllers
{
    public class BasketController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public BasketController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            List<BasketItemVM> basketVM = new();

            if (User.Identity.IsAuthenticated)
            {
                basketVM = await _context.BasketItems
                    .Where(bi => bi.AppUserId == User.FindFirstValue(ClaimTypes.NameIdentifier))
                    .Select(bi => new BasketItemVM
                    {
                        Count = bi.Count,
                        Price = bi.Product.Price,
                        Image = bi.Product.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true).ImageUrl,
                        Name = bi.Product.Name,
                        SubTotal = bi.Product.Price * bi.Count,
                        Id = bi.ProductId
                    })
                    .ToListAsync();
            }
            else
            {
                List<BasketCookieItemVM> cookiesVM;
                string cookie = Request.Cookies["basket"];


                if (cookie == null)
                {
                    return View(basketVM);
                }

                cookiesVM = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(cookie);

                foreach (BasketCookieItemVM item in cookiesVM)
                {
                    Product product = await _context.Products
                        .Include(p => p.ProductImages.Where(pi => pi.IsPrimary == true))
                        .FirstOrDefaultAsync(p => p.Id == item.Id);

                    if (product != null)
                    {
                        basketVM.Add(new BasketItemVM
                        {
                            Id = product.Id,
                            Name = product.Name,
                            Image = product.ProductImages[0].ImageUrl,
                            Price = product.Price,
                            Count = item.Count,
                            SubTotal = item.Count * product.Price
                        });
                    }
                }
            }





            return View(basketVM);
        }

        public async Task<IActionResult> AddBasket(int? id)
        {
            if (id == null || id < 1) { return BadRequest(); }

            bool result = await _context.Products.AnyAsync(p => p.Id == id);

            if (!result) { return NotFound(); }


            if (User.Identity.IsAuthenticated)
            {
                AppUser? user = await _userManager.Users
                    .Include(u => u.BasketItems)
                    .FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

                BasketItem item = user.BasketItems.FirstOrDefault(bi => bi.ProductId == id);

                if (item == null)
                {
                    user.BasketItems.Add(new BasketItem
                    {
                        ProductId = id.Value,
                        Count = 1
                    });
                }
                else
                {
                    item.Count++;
                }

                await _context.SaveChangesAsync();

            }
            else
            {
                List<BasketCookieItemVM> basket;
                string cookies = Request.Cookies["basket"];

                if (cookies != null)
                {
                    basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(cookies);

                    BasketCookieItemVM existed = basket.FirstOrDefault(b => b.Id == id);
                    if (existed != null)
                    {
                        existed.Count++;
                    }
                    else
                    {
                        basket.Add(new BasketCookieItemVM()
                        {
                            Id = id.Value,
                            Count = 1
                        });
                    }
                }
                else
                {
                    basket = new List<BasketCookieItemVM>();
                    basket.Add(new BasketCookieItemVM()
                    {
                        Id = id.Value,
                        Count = 1
                    });
                }

                string json = JsonConvert.SerializeObject(basket);

                Response.Cookies.Append("basket", json);
            }


            return RedirectToAction("Index", "Home");
        }


        [Authorize(Roles = "Member")]
        public async Task<IActionResult> Checkout()
        {
            OrderVM orderVM = new()
            {
                BasketInOrderItemsVMs = await _context.BasketItems
                .Where(bi => bi.AppUserId == User.FindFirstValue(ClaimTypes.NameIdentifier))
                .Select(bi => new BasketInOrderItemVM
                {
                    Count = bi.Count,
                    Name = bi.Product.Name,
                    Price = bi.Product.Price,
                    Subtotal = bi.Product.Price * bi.Count
                })
                .ToListAsync()
            };

            return View(orderVM);
        }


        [HttpPost]
        public async Task<IActionResult> Checkout(OrderVM orderVM)
        {

            List<BasketItem> basketItems = await _context.BasketItems
            .Where(bi => bi.AppUserId == User.FindFirstValue(ClaimTypes.NameIdentifier))
            .Include(bi => bi.Product)
            .ToListAsync();

            if (!ModelState.IsValid)
            {
                orderVM.BasketInOrderItemsVMs = basketItems
                    .Select(bi => new BasketInOrderItemVM
                    {
                        Count = bi.Count,
                        Name = bi.Product.Name,
                        Price = bi.Product.Price,
                        Subtotal = bi.Product.Price * bi.Count
                    })
                .ToList();

                return View(orderVM);
            }

            Order order = new Order()
            {
                Address = orderVM.Address,
                Status = null,
                AppUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                CreatedAt = DateTime.Now,
                DateString = DateTime.Now.ToString("f"),
                IsDeleted = false,
                OrderItems = basketItems.Select(bi => new OrderItem
                {
                    AppUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    Count = bi.Count,
                    Price = bi.Product.Price,
                    ProductId = bi.ProductId
                }).ToList(),

                TotalPrice = basketItems.Sum(bi => bi.Product.Price * bi.Count)
            };

            await _context.Orders.AddAsync(order);
            _context.BasketItems.RemoveRange(basketItems);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
