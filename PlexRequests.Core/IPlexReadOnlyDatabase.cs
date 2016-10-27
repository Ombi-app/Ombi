using System;
using System.Collections.Generic;
using PlexRequests.Store.Models.Plex;

namespace PlexRequests.Core
{
    public interface IPlexReadOnlyDatabase
    {
        IEnumerable<MetadataItems> GetItemsAddedAfterDate(DateTime dateTime);
    }
}