using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    [Table("RequestQueue")]
    public class RequestQueue : Entity
    {
        public int RequestId { get; set; }
        public RequestType Type { get; set; }
        public DateTime Dts { get; set; }
        public string Error { get; set; }
        public DateTime? Completed { get; set; }
        public int RetryCount { get; set; }
    }
}