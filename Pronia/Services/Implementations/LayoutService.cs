using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pronia.DAL;
using Pronia.Models;
using Pronia.Services.Interfaces;
using Pronia.ViewModels.Basket;

namespace Pronia.Services.Implementations
{
    public class LayoutService : ILayoutService
    {
        private readonly AppDbContext _context;
        private readonly HttpContext _http;

        public LayoutService(AppDbContext context, IHttpContextAccessor http)
        {
            _context = context;
            _http = http.HttpContext;
        }

        public async Task<List<BasketItemVM>> GetBasketAsync()
        {

            List<BasketCookieItemVM> cookiesVM;
            string cookie = _http.Request.Cookies["basket"];

            List<BasketItemVM> basketVM = new();

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


            return basketVM;
        }

        public async Task<Dictionary<string, string>> GetSettingsAsync()
        {
            Dictionary<string, string> settings = await _context.LayoutSettings.ToDictionaryAsync(s => s.Key, s => s.Value);

            return settings;
        }
    }
}
