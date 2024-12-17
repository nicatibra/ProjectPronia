using Microsoft.AspNetCore.Mvc;

namespace Pronia.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Index(string errorMessage)
        {
            return View(model: errorMessage);
        }
    }
}
