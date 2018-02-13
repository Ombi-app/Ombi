using Newtonsoft.Json;
using Ombi.Helpers;

namespace Ombi.Api.SickRage.Models
{
    public class SickRageSeasonList : SickRageBase<object>
    {
        [JsonIgnore]
        public int[] Data => JsonConvertHelper.ParseObjectToArray<int>(data);
    }
}