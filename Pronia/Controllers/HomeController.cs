using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.DAL;
using Pronia.ViewModels;

namespace Pronia.Controllers
{
    public class HomeController : Controller
    {
        public readonly AppDbContext _context;
        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {

            #region ForAddingToDB
            //Slide slide1 = new Slide("TORCH FLOWER", "50% OFF", "100% from Minecraft", "TorchFlower.png", 2, false, DateTime.Now);
            //Slide slide2 = new Slide("PITCHER PLANT", "60% OFF", "100% from Minecraft", "PitcherPlant.webp", 1, false, DateTime.Now);
            //Slide slide3 = new Slide("ALLIUM", "40% OFF", "100% from Minecraft", "Allium.webp", 3, false, DateTime.Now);

            //List<Slide> slides = new List<Slide>();
            //slides.Add(slide1);
            //slides.Add(slide2);
            //slides.Add(slide3);

            //_context.Slides.AddRange(slides);
            //_context.SaveChanges(); 
            #endregion


            HomeVM homeVM = new HomeVM
            {
                Slides = await _context.Slides
                .Where(p => p.IsDeleted == false)//gelecekde heresine aid bele tek tek IsDeleted False serti qoyulmayacaq(silinecek)
                .OrderBy(s => s.Order)
                .Take(3)
                .ToListAsync(),


                Products = await _context.Products
                .Where(p => p.IsDeleted == false)//Bu da silinecek
                .Take(8)
                .Include(p => p.ProductImages.Where(pi => pi.IsPrimary != null))
                .ToListAsync()
            };

            return View(homeVM);
        }
    }
}
