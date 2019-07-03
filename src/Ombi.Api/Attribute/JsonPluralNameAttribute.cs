using System;

namespace Ombi.Api
{
    public class JsonPropertyNameBasedOnItemClassAttribute : Attribute
    {
    }

    public class JsonPluralNameAttribute : Attribute
    {
        public string PluralName { get; set; }
        public JsonPluralNameAttribute(string pluralName)
        {
            PluralName = pluralName;
        }
    }
}