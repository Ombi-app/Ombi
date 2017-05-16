using Microsoft.AspNetCore.Mvc;

namespace Ombi.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
