using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Store.Models.Plex;

namespace Ombi.Store
{
    public interface IPlexDatabase
    {
        IEnumerable<MetadataItems> GetMetadata();
        string DbLocation { get; set; }
        Task<IEnumerable<MetadataItems>> GetMetadataAsync();
        IEnumerable<MetadataItems> QueryMetadataItems(string query, object param);
    }
}