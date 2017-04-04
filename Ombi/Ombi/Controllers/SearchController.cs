using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Ombi.Controllers
{
    public class SearchController : Controller
    {
        [Route()]
        public IActionResult Index()
        {
            return View();
        }

       
    }
}
