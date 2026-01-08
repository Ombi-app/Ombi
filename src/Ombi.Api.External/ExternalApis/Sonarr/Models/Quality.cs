namespace Ombi.Api.External.ExternalApis.Sonarr.Models
{
    public class Quality
    {
        public Quality()
        {
            
        }

        public Quality(Quality q)
        {
            id = q.id;
            name = q.name;
        }
        public int id { get; set; }
        public string name { get; set; }
    }
}