using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Core.Processor;
using Ombi.Helpers;

namespace Ombi.Controllers.V1
{
    [ApiV1]
    [Produces("application/json")]
    [AllowAnonymous]
    [ApiController]
    public class UpdateController : ControllerBase
    {
        public UpdateController(ICacheService cache, IChangeLogProcessor processor)
        {
            _cache = cache;
            _processor = processor;
        }

        private readonly ICacheService _cache;
        private readonly IChangeLogProcessor _processor;

        [HttpGet("{branch}")]
        public async Task<UpdateModel> UpdateAvailable(string branch)
        {
            return await _cache.GetOrAdd(branch, async () => await _processor.Process(branch));
        }
    }
}