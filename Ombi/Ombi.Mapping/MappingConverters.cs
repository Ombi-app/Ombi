using AutoMapper;
using System;
using System.Security.Claims;
using Ombi.Core.Models.UI;

namespace Ombi.Mapping
{
    public class StringToDateTimeConverter : ITypeConverter<string, DateTime>
    {

        public DateTime Convert(string source, DateTime destination, ResolutionContext context)
        {
            DateTime dateTime;

            if (string.IsNullOrEmpty(source))
            {
                return default(DateTime);
            }

            if (DateTime.TryParse(source.ToString(), out dateTime))
            {
                return dateTime;
            }

            return default(DateTime);
        }
    }

    public class ClaimsConverter : ITypeConverter<Claim, ClaimCheckboxes>
    {

        public ClaimCheckboxes Convert(Claim source, ClaimCheckboxes destination, ResolutionContext context)
        {
            return new ClaimCheckboxes
            {
                Enabled = true,
                Value = source.Value
            };
        }
    }
}