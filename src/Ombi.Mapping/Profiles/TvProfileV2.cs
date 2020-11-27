using System.Globalization;
using AutoMapper;
using Ombi.Api.TvMaze.Models.V2;
using Ombi.Core.Models.Search;
using Ombi.Core.Models.Search.V2;
using Ombi.Helpers;

namespace Ombi.Mapping.Profiles
{
    public class TvProfileV2 : Profile
    {
        public TvProfileV2()
        {
            CreateMap<FullSearch, SearchFullInfoTvShowViewModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.externals.thetvdb))
                .ForMember(dest => dest.FirstAired, opts => opts.MapFrom(src => src.premiered))
                .ForMember(dest => dest.ImdbId, opts => opts.MapFrom(src => src.externals.imdb))
                .ForMember(dest => dest.Network, opts => opts.MapFrom(src => src.network.name))
                .ForMember(dest => dest.NetworkId, opts => opts.MapFrom(src => src.network.id.ToString()))
                .ForMember(dest => dest.Overview, opts => opts.MapFrom(src => src.summary.RemoveHtml()))
                .ForMember(dest => dest.Rating,
                    opts => opts.MapFrom(src => src.rating.average.ToString(CultureInfo.CurrentUICulture)))
                .ForMember(dest => dest.Runtime, opts => opts.MapFrom(src => src.runtime.ToString()))
                .ForMember(dest => dest.SeriesId, opts => opts.MapFrom(src => src.id))
                .ForMember(dest => dest.Title, opts => opts.MapFrom(src => src.name))
                .ForMember(dest => dest.Network, opts => opts.MapFrom(src => src.network))
                .ForMember(dest => dest.Images, opts => opts.MapFrom(src => src.image))
                .ForMember(dest => dest.Cast, opts => opts.MapFrom(src => src._embedded.cast))
                .ForMember(dest => dest.Crew, opts => opts.MapFrom(src => src._embedded.crew))
                .ForMember(dest => dest.Banner,
                    opts => opts.MapFrom(src =>
                        !string.IsNullOrEmpty(src.image.medium)
                            ? src.image.medium.ToHttpsUrl()
                            : string.Empty))
                .ForMember(dest => dest.Status, opts => opts.MapFrom(src => src.status));

            CreateMap<Network, NetworkViewModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.id))
                .ForMember(dest => dest.Country, opts => opts.MapFrom(src => src.country))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.name));

            CreateMap<Api.TvMaze.Models.V2.Country, Core.Models.Search.V2.Country>()
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.name))
                .ForMember(dest => dest.Code, opts => opts.MapFrom(src => src.code))
                .ForMember(dest => dest.Timezone, opts => opts.MapFrom(src => src.timezone));

            CreateMap<Api.TvMaze.Models.V2.Image, Images>()
                .ForMember(dest => dest.Medium, opts => opts.MapFrom(src => src.medium.ToHttpsUrl()))
                .ForMember(dest => dest.Original, opts => opts.MapFrom(src => src.original.ToHttpsUrl()));

            CreateMap<Api.TvMaze.Models.V2.Cast, CastViewModel>()
                .ForMember(dest => dest.Character, opts => opts.MapFrom(src => src.character))
                .ForMember(dest => dest.Person, opts => opts.MapFrom(src => src.person))
                .ForMember(dest => dest.Voice, opts => opts.MapFrom(src => src.voice))
                .ForMember(dest => dest.Self, opts => opts.MapFrom(src => src.self));

            CreateMap<Api.TvMaze.Models.V2.Person, PersonViewModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.id))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.name))
                .ForMember(dest => dest.Image, opts => opts.MapFrom(src => src.image))
                .ForMember(dest => dest.Url, opts => opts.MapFrom(src => src.url.ToHttpsUrl()));

            CreateMap<Api.TvMaze.Models.V2.Crew, CrewViewModel>()
                .ForMember(dest => dest.Person, opts => opts.MapFrom(src => src.person))
                .ForMember(dest => dest.Type, opts => opts.MapFrom(src => src.type));

            CreateMap<Api.TvMaze.Models.V2.Cast, CastViewModel>()
                .ForMember(dest => dest.Person, opts => opts.MapFrom(src => src.person))
                .ForMember(dest => dest.Self, opts => opts.MapFrom(src => src.self))
                .ForMember(dest => dest.Voice, opts => opts.MapFrom(src => src.voice))
                .ForMember(dest => dest.Character, opts => opts.MapFrom(src => src.character));

            CreateMap<Api.TvMaze.Models.V2.Character, CharacterViewModel>()
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.name))
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.id))
                .ForMember(dest => dest.Url, opts => opts.MapFrom(src => src.url.ToHttpsUrl()))
                .ForMember(dest => dest.Image, opts => opts.MapFrom(src => src.image));

            CreateMap<SearchTvShowViewModel, SearchFullInfoTvShowViewModel>().ReverseMap();
        }
    }
}