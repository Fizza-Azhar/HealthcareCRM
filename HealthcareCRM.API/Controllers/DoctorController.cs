using Microsoft.AspNetCore.Mvc;

namespace HealthcareCRM.API.Controllers
{
    public class DoctorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}