#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: StatusController.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion

using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models;
using Ombi.Core.Update;
using Ombi.Settings.Settings.Models;

namespace Ombi.Controllers
{
    [ApiV1]
    [Produces("application/json")]
    public class StatusController : Controller
    {
        public StatusController(ISettingsService<OmbiSettings> ombi)
        {
            Ombi = ombi;
        }

        private ISettingsService<OmbiSettings> Ombi { get; }

        /// <summary>
        /// Gets the status of Ombi.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public HttpStatusCode GetStatus()
        {
            return HttpStatusCode.OK;
        }


        /// <summary>
        /// Checks to see if we have run through the wizard
        /// </summary>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [AllowAnonymous]
        [HttpGet("Wizard")]
        public async Task<object> WizardStatus()
        {
            var settings = await Ombi.GetSettingsAsync();

            return new { Result = settings?.Wizard ?? false};
        }

        [AllowAnonymous]
        [HttpGet("Update")]
        public async Task Update()
        {
            var u = new UpdateEngine();
            var result = u.CheckForUpdate();
            await u.Update(result);
        }

    }
}