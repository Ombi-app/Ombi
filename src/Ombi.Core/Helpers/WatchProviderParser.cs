using Ombi.Api.TheMovieDb.Models;
using Ombi.Store.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ombi.Core.Helpers
{
    public static class WatchProviderParser
    {
        public static List<StreamData> GetUserWatchProviders(WatchProviders providers, OmbiUser user)
        {
            var data = new List<StreamData>();

            if (providers?.Results == null)
            {
                return data;
            }

            var resultsProp = providers.Results.GetType().GetProperties();
            var matchingStreamingCountry = resultsProp.FirstOrDefault(x => x.Name.Equals(user.StreamingCountry, StringComparison.InvariantCultureIgnoreCase));
            if (matchingStreamingCountry == null)
            {
                return data;
            }

            var result = (WatchProviderData)matchingStreamingCountry.GetValue(providers.Results);
            if (result == null || result.StreamInformation == null)
            {
                return data;
            }
            return result.StreamInformation;
        }
    }
}
