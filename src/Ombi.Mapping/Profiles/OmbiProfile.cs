using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using AutoMapper;
using Ombi.Core.Models;
using Ombi.Core.Models.UI;
using Ombi.Store.Entities;

namespace Ombi.Mapping.Profiles
{
    public class OmbiProfile : Profile
    {
        public OmbiProfile()
        {
            //CreateMap<User, UserDto>().ReverseMap();


            CreateMap<Claim, ClaimCheckboxes>().ConvertUsing<ClaimsConverter>();

            CreateMap<OmbiUser, UserViewModel>().ForMember(x => x.Password, opt => opt.Ignore());

            CreateMap<ClaimCheckboxes, Claim>()
                .ConstructUsing(checkbox => checkbox.Enabled ? new Claim(ClaimTypes.Role, checkbox.Value) : new Claim(ClaimTypes.Country, ""));
                // This is used for the UserViewModel List<string> claims => UserDto List<claim>
            CreateMap<UserViewModel, UserDto>();

            CreateMap<string, DateTime>().ConvertUsing<StringToDateTimeConverter>();
        }
    }
}
