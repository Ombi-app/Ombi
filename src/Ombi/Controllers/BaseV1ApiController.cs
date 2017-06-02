using Microsoft.AspNetCore.Mvc;

namespace Ombi.Controllers
{
    [Route(ApiBase)]
    public class BaseV1ApiController : Controller
    {
        protected const string ApiBase = "api/v1/[controller]";
    }
}