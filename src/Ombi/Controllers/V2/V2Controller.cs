using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ombi.Controllers.V2
{
    
    [ApiV2]
    [Authorize]
    [ApiController]
    public class V2Controller : ControllerBase
    {
        
    }
}