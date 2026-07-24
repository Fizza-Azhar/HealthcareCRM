using Microsoft.AspNetCore.Mvc;

namespace HealthcareCRM.API.Controllers
{
    public class DashboardViewController : Controller
    {
        [HttpGet]
        [Route("Dashboard")]
        public IActionResult Index()
        {
            return View("~/Views/Dashboard/Index.cshtml");
        }
    }
}