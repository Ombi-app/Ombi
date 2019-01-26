using Microsoft.AspNetCore.Mvc;

namespace Ombi.Controllers
{
    [Route(ApiBase)]
    public class ApiV2Attribute : RouteAttribute
    {
        protected const string ApiBase = "api/v2/[controller]";

        public ApiV2Attribute() : base(ApiBase)
        {
        }
    }
}