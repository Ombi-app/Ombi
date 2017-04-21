using System;
using AutoMapper;
using Ombi.Core.Models;
using Ombi.Store.Entities;

namespace Ombi.Mapping.Profiles
{
    public class OmbiProfile : Profile
    {
        public OmbiProfile()
        {
            CreateMap<User, UserDto>().ReverseMap();

            CreateMap<string, DateTime>().ConvertUsing<StringToDateTimeConverter>();
        }
    }
}
