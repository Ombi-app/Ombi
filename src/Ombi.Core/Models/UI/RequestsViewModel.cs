using System.Collections.Generic;

namespace Ombi.Core.Models.UI
{
    public class RequestsViewModel<T>
    {
        public IEnumerable<T> Collection { get; set; }
        public int Total { get; set; }
    }
}