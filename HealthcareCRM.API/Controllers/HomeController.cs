using Microsoft.AspNetCore.Mvc;

namespace HealthcareCRM.API.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}