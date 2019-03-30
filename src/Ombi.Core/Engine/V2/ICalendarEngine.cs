using Ombi.Core.Models.Search.V2;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Ombi.Core.Engine.V2
{
    public interface ICalendarEngine
    {
        Task<List<CalendarViewModel>> GetCalendarData();
    }
}