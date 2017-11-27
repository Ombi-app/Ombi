namespace Ombi.Models
{
    public class MediaSeverAvailibilityViewModel
    {
        public int ServersAvailable { get; set; }
        public int ServersUnavailable { get; set; }
        public bool PartiallyDown => ServersUnavailable > 0 && ServersAvailable > 0;
        public bool CompletelyDown => ServersUnavailable > 0 && ServersAvailable == 0;
        public bool FullyAvailable => ServersUnavailable == 0 && ServersAvailable > 0;
        public int TotalServers => ServersUnavailable + ServersAvailable;
    }
}