using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Ombi.Attributes;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ombi.Controllers.V2
{
    [ServiceFilter(typeof(WizardActionFilter))]
    [AllowAnonymous]
    public class WizardController : V2Controller
    {

        private ISettingsService<OmbiSettings> _ombiSettings { get; }
        

        [HttpGet]
        public IActionResult Ok()
        {
            return Ok();
        }




    }


}
