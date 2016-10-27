using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using PlexRequests.Store.Models.Plex;

namespace PlexRequests.Store
{
    public interface IPlexDatabase
    {
        IEnumerable<MetadataItems> GetMetadata();
        string DbLocation { get; set; }
        Task<IEnumerable<MetadataItems>> GetMetadataAsync();
        IEnumerable<MetadataItems> QueryMetadataItems(string query, object param);
    }
}