using Newtonsoft.Json;
using Ombi.Helpers;

namespace Ombi.Api.External.ExternalApis.SickRage.Models
{
    public class SickRageSeasonList : SickRageBase<object>
    {
        [JsonIgnore]
        public int[] Data => JsonConvertHelper.ParseObjectToArray<int>(data);
    }
}