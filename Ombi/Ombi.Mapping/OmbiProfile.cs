using System;
using AutoMapper;
using Ombi.Core.Models;
using Ombi.Store.Entities;

namespace Ombi.Mapping
{
    public class OmbiProfile : Profile
    {
        public OmbiProfile()
        {
            CreateMap<User, UserDto>().ReverseMap();
        }
    }
}
