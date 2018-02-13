namespace Ombi.Api.SickRage.Models
{
    public class SickRageShowInformation : SickRageBase<SickRageShowInformationData>
    {

    }

    public class SickRageShowInformationData
    {
        public int air_by_date { get; set; }
        public string airs { get; set; }
        public Cache cache { get; set; }
        public int flatten_folders { get; set; }
        public string[] genre { get; set; }
        public string language { get; set; }
        public string location { get; set; }
        public string network { get; set; }
        public string next_ep_airdate { get; set; }
        public int paused { get; set; }
        public string quality { get; set; }
        public Quality_Details quality_details { get; set; }
        public int[] season_list { get; set; }
        public string show_name { get; set; }
        public string status { get; set; }
        public int tvrage_id { get; set; }
        public string tvrage_name { get; set; }
    }

    public class Cache
    {
        public int banner { get; set; }
        public int poster { get; set; }
    }

    public class Quality_Details
    {
        public object[] archive { get; set; }
        public string[] initial { get; set; }
    }

}