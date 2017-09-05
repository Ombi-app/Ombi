using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Ombi.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : Controller
    {
        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
                     return View();
        }
    }
}
