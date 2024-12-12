using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pronia.DAL;
using Pronia.Models;
using Pronia.Services.Interfaces;
using Pronia.ViewModels.Basket;
using System.Security.Claims;

namespace Pronia.Services.Implementations
{
    public class LayoutService : ILayoutService
    {
        private readonly AppDbContext _context;
        private readonly HttpContext _http;
        private readonly ClaimsPrincipal _user;

        public LayoutService(AppDbContext context, IHttpContextAccessor http)
        {
            _context = context;
            _http = http.HttpContext;
            _user = _http.User;
        }

        public async Task<List<BasketItemVM>> GetBasketAsync()
        {
            List<BasketItemVM> basketVM = new();

            if (_user.Identity.IsAuthenticated)
            {
                basketVM = await _context.BasketItems
                   .Where(bi => bi.AppUserId == _user.FindFirstValue(ClaimTypes.NameIdentifier))
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
                string cookie = _http.Request.Cookies["basket"];


                if (cookie == null)
                {
                    return basketVM;
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




            return basketVM;
        }

        public async Task<Dictionary<string, string>> GetSettingsAsync()
        {
            Dictionary<string, string> settings = await _context.LayoutSettings.ToDictionaryAsync(s => s.Key, s => s.Value);

            return settings;
        }
    }
}
