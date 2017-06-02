using System.Collections.Generic;
using Ombi.Core.Models.Requests;

namespace Ombi.Models
{
    public class RequestGridModel<T> where T : BaseRequestModel
    {
        public IEnumerable<T> Available { get; set; }
        public IEnumerable<T> New { get; set; }
        public IEnumerable<T> Approved { get; set; }
    }
}