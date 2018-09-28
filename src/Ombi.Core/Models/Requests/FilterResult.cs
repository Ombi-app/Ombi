using System.Collections.Generic;

namespace Ombi.Core.Models.Requests
{
    public class FilterResult<T>
    {
        public int Total { get; set; }
        public IEnumerable<T> Collection { get; set; }
    }
}