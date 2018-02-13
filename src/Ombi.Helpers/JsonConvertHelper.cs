using Newtonsoft.Json;

namespace Ombi.Helpers
{
    public static class JsonConvertHelper
    {
        public static T[] ParseObjectToArray<T>(object ambiguousObject)
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