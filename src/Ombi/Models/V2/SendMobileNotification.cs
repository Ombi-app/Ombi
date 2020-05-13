using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ombi.Models.V2
{
    public class SendMobileNotification
    {
        public string UserId { get; set; }
        public string Message { get; set; }
    }
}
