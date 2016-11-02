using Newtonsoft.Json;
using PlexRequests.Helpers;

namespace PlexRequests.Api.Models.SickRage
{
    public class SickRageSeasonList : SickRageBase<object>
    {
        [JsonIgnore]
        public int[] Data => JsonConvertHelper.ParseObjectToArray<int>(data);
    }
}