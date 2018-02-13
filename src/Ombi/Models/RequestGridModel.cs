using System.Collections.Generic;
using Ombi.Store.Entities.Requests;

namespace Ombi.Models
{
    public class RequestGridModel<T> where T : BaseRequest
    {
        public IEnumerable<T> Available { get; set; }
        public IEnumerable<T> New { get; set; }
        public IEnumerable<T> Approved { get; set; }
    }
}