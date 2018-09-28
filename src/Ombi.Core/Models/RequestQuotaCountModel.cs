using System;

namespace Ombi.Core.Models
{
    public class RequestQuotaCountModel
    {
        public bool HasLimit { get; set; }

        public int Limit { get; set; }

        public int Remaining { get; set; }

        public DateTime NextRequest { get; set; }
    }
}