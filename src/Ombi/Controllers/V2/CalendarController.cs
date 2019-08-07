using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

using Ombi.Core.Engine.V2;
using Ombi.Core.Models.Search.V2;

namespace Ombi.Controllers.V2
{
    public class CalendarController : V2Controller
    {
        public CalendarController(ICalendarEngine calendarEngine)
        {
            _calendarEngine = calendarEngine;
        }

        private readonly ICalendarEngine _calendarEngine;


        [HttpGet]
        public async Task<List<CalendarViewModel>> GetCalendarEntries()
        {
            return await _calendarEngine.GetCalendarData();
        }
    }
}