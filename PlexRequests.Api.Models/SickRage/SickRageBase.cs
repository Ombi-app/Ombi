using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlexRequests.Api.Models.SickRage
{
    public class SickRageBase<T>
    {
        public T data { get; set; }
        public string message { get; set; }
        public string result { get; set; }
    }
}
