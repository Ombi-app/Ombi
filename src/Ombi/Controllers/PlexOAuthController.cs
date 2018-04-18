using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Internal;
using System.IO;
using System.Text;

namespace Ombi.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiV1]
    [AllowAnonymous]
    public class PlexOAuthController : Controller
    {

        [HttpGet]
        public IActionResult OAuthCallBack()
        {
            var bodyStr = "";
            var req = Request;

            // Allows using several time the stream in ASP.Net Core
            req.EnableRewind();

            // Arguments: Stream, Encoding, detect encoding, buffer size 
            // AND, the most important: keep stream opened
            using (StreamReader reader
                = new StreamReader(req.Body, Encoding.UTF8, true, 1024, true))
            {
                bodyStr = reader.ReadToEnd();
            }

            // Rewind, so the core is not lost when it looks the body for the request
            req.Body.Position = 0;

            // Do your work with bodyStr
            return Ok();
        }
    }
}
