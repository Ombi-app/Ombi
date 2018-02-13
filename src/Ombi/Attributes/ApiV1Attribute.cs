using Microsoft.AspNetCore.Mvc;

namespace Ombi.Controllers
{
    [Route(ApiBase)]
    public class ApiV1Attribute : RouteAttribute
    {
        protected const string ApiBase = "api/v1/[controller]";

        public ApiV1Attribute() : base(ApiBase)
        {
        }
    }
}