﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Core.Processor;
using Ombi.Helpers;

namespace Ombi.Controllers
{
    [ApiV1]
    [Produces("application/json")]
    [AllowAnonymous]
    public class UpdateController : Controller
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