using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PlexRequests.Helpers;

namespace PlexRequests.Api.Models.SickRage
{
    public class SickRageSeasonList : SickRageBase<object>
    {
        [JsonIgnore]
        public int[] Data => ParseObjectToArray<int>(data);

        protected T[] ParseObjectToArray<T>(object ambiguousObject)
        {
            var json = ambiguousObject.ToString();
            if (string.IsNullOrWhiteSpace(json))
            {
                return new T[0]; // Could return null here instead.
            }
            if (json.TrimStart().StartsWith("["))
            {
                return JsonConvert.DeserializeObject<T[]>(json);
            }
            if (json.TrimStart().Equals("{}"))
            {
                return new T[0];
            }

            return new T[1] { JsonConvert.DeserializeObject<T>(json) };

        }
    }
}