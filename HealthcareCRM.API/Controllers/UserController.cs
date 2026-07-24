using Microsoft.AspNetCore.Mvc;

namespace HealthcareCRM.API.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}