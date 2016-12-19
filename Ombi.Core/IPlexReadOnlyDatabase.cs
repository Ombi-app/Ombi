using System;
using System.Collections.Generic;
using Ombi.Store.Models.Plex;

namespace Ombi.Core
{
    public interface IPlexReadOnlyDatabase
    {
        IEnumerable<MetadataItems> GetItemsAddedAfterDate(DateTime dateTime);
    }
}