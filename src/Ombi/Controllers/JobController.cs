using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Ombi.Api.Service;
using Ombi.Attributes;
using Ombi.Schedule.Ombi;

namespace Ombi.Controllers
{
    [ApiV1]
    [Admin]
    [Produces("application/json")]
    public class JobController : Controller
    {
        public JobController(IOmbiAutomaticUpdater updater)
        {
            _updater = updater;
        }

        private readonly IOmbiAutomaticUpdater _updater;

        [HttpPost("update")]
        public bool ForceUpdate()
        {
            BackgroundJob.Enqueue(() => _updater.Update(null));
            return true;
        }

        [HttpGet("update")]
        public async Task<bool> CheckForUpdate()
        {
            var productArray = _updater.GetVersion();
            var version = productArray[0];
            var branch = productArray[1];
            var updateAvailable = await _updater.UpdateAvailable(branch, version);

            return updateAvailable;
        }
    }
}