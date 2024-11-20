using Microsoft.AspNetCore.Mvc;

namespace Pronia.Controllers
{
    public class ShopController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
