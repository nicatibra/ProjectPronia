using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pronia.DAL;
using Pronia.ViewModels;
using Pronia.ViewModels.Basket;
using System.Security.Claims;

namespace Pronia.Services.Implementations
{
    public class BasketService : IBasketService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _http;
        private readonly ClaimsPrincipal _user;

        public BasketService(AppDbContext context, IHttpContextAccessor http)
        {
            _context = context;
            _http = http;
            _user = http.HttpContext.User;
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
                string cookie = _http.HttpContext.Request.Cookies["basket"];


                if (cookie == null)
                {
                    return basketVM;
                }

                cookiesVM = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(cookie);

                #region AlphaVersion
                //foreach (BasketCookieItemVM item in cookiesVM)
                //{
                //    Product product = await _context.Products
                //        .Include(p => p.ProductImages.Where(pi => pi.IsPrimary == true))
                //        .FirstOrDefaultAsync(p => p.Id == item.Id);

                //    if (product != null)
                //    {
                //        basketVM.Add(new BasketItemVM
                //        {
                //            Id = product.Id,
                //            Name = product.Name,
                //            Image = product.ProductImages[0].ImageUrl,
                //            Price = product.Price,
                //            Count = item.Count,
                //            SubTotal = item.Count * product.Price
                //        });
                //    }
                //} 
                #endregion

                basketVM = await _context.Products.Where(p => cookiesVM.Select(c => c.Id).Contains(p.Id)).Select(p => new BasketItemVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Image = p.ProductImages[0].ImageUrl,
                    Price = p.Price,
                }).ToListAsync();

                basketVM.ForEach(bi =>
                {
                    bi.Count = cookiesVM.FirstOrDefault(c => c.Id == bi.Id).Count;
                    bi.SubTotal = bi.Price * bi.Count;
                });

            }

            return basketVM;
        }
    }
}
