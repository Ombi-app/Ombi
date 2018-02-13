using System;
using System.Text;
using Newtonsoft.Json;

namespace Ombi.Helpers
{
    public class ByteConverterHelper
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };
        public static byte[] ReturnBytes(object obj)
        {
            var json = JsonConvert.SerializeObject(obj, Settings);
            var bytes = Encoding.UTF8.GetBytes(json);

            return bytes;
        }

        public static T ReturnObject<T>(byte[] bytes)
        {
            var json = Encoding.UTF8.GetString(bytes);
            var model = JsonConvert.DeserializeObject<T>(json, Settings);
            return model;
        }
        public static string ReturnFromBytes(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
