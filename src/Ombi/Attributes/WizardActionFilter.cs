using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models;
using System.Threading.Tasks;

namespace Ombi.Attributes
{
    public class WizardActionFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var settingsService = context.HttpContext.RequestServices.GetRequiredService<ISettingsService<OmbiSettings>>();

            var settings = await settingsService.GetSettingsAsync();

            if (!settings.Wizard)
            {
                await next();
                return;
            }
            context.Result = new UnauthorizedResult();
        }
    }
}
