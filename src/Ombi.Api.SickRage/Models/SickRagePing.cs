namespace Ombi.Api.SickRage.Models
{
    public class SickRagePingData
    {
        public int pid { get; set; }
    }

    public class SickRagePing : SickRageBase<SickRagePingData>
    {
    }
}