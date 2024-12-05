using Microsoft.AspNetCore.Mvc;
using Pronia.Services.Interfaces;

namespace Pronia.Controllers
{
    public class EmailController : Controller
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            await _emailService.SendMailAsync("nicatfi-bp217@code.edu.az", "Your Order", "Hell Yeah, Email sent Successfully", true);
            return RedirectToAction("Index", "Home");
        }
    }
}
