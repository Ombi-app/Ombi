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
            CreateMap<User, UserDto>().ReverseMap();


            CreateMap<UserDto, UserViewModel>()
                .ForMember(dest => dest.Claims, opts => opts.MapFrom(src => src.Claims.Select(x => x.Value).ToList())); // Map the claims to a List<string>

            CreateMap<string, Claim>()
                .ConstructUsing(str => new Claim(ClaimTypes.Role, str)); // This is used for the UserViewModel List<string> claims => UserDto List<claim>
            CreateMap<UserViewModel, UserDto>();

            CreateMap<string, DateTime>().ConvertUsing<StringToDateTimeConverter>();
        }
    }
}
