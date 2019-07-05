using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;

namespace Ombi.Api
{
    public class PluralPropertyContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty prop = base.CreateProperty(member, memberSerialization);
            if (prop.PropertyType.IsGenericType && member.GetCustomAttribute<JsonPropertyNameBasedOnItemClassAttribute>() != null)
            {
                Type itemType = prop.PropertyType.GetGenericArguments()[0];
                JsonPluralNameAttribute att = itemType.GetCustomAttribute<JsonPluralNameAttribute>();
                prop.PropertyName = att != null ? att.PluralName : Pluralize(itemType.Name);
            }
            return prop;
        }

        protected string Pluralize(string name)
        {
            if (name.EndsWith("y") && !name.EndsWith("ay") && !name.EndsWith("ey") && !name.EndsWith("oy") && !name.EndsWith("uy"))
                return name.Substring(0, name.Length - 1) + "ies";

            if (name.EndsWith("s"))
                return name + "es";

            return name + "s";
        }
    }
}