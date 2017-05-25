namespace Ombi.Core.Models.Requests
{
    public class RequestCountModel
    {
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Available { get; set; }
    }
}