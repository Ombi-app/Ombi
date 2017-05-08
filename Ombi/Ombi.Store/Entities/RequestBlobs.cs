using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    [Table("RequestBlobs")]
    public class RequestBlobs : Entity
    {
        public int ProviderId { get; set; }
        public byte[] Content { get; set; }
        public RequestType Type { get; set; }

    }
    public enum RequestType
    {
        Movie = 1,
        TvShow = 2
    }
}