using System;
using System.Text;
using Newtonsoft.Json;

namespace Ombi.Helpers
{
    public class ByteConverterHelper
    {
        public static byte[] ReturnBytes(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            var bytes = Encoding.UTF8.GetBytes(json);

            return bytes;
        }

        public static T ReturnObject<T>(byte[] bytes)
        {
            var json = Encoding.UTF8.GetString(bytes);
            var model = JsonConvert.DeserializeObject<T>(json);
            return model;
        }
        public static string ReturnFromBytes(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
