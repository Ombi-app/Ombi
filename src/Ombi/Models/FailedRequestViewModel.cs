using System;
using Ombi.Store.Entities;

namespace Ombi.Models
{
    public class FailedRequestViewModel
    {
        public int FailedId { get; set; }
        public string Title { get; set; }
        public DateTime ReleaseYear { get; set; }
        public int RequestId { get; set; }
        public RequestType Type { get; set; }
        public DateTime Dts { get; set; }
        public string Error { get; set; }
        public int RetryCount { get; set; }
    }
}