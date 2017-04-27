using System.Globalization;
using AutoMapper;
using Ombi.Api.TvMaze.Models;
using Ombi.Core.Models.Search;
using Ombi.Helpers;

namespace Ombi.Mapping.Profiles
{
    public class TvProfile : Profile
    {
        public TvProfile()
        {
            CreateMap<TvMazeSearch, SearchTvShowViewModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.show.externals.thetvdb))
                .ForMember(dest => dest.FirstAired, opts => opts.MapFrom(src => src.show.premiered))
                .ForMember(dest => dest.ImdbId, opts => opts.MapFrom(src => src.show.externals.imdb))
                .ForMember(dest => dest.Network, opts => opts.MapFrom(src => src.show.network.name))
                .ForMember(dest => dest.NetworkId, opts => opts.MapFrom(src => src.show.network.id.ToString()))
                .ForMember(dest => dest.Overview, opts => opts.MapFrom(src => src.show.summary.RemoveHtml()))
                .ForMember(dest => dest.Rating, opts => opts.MapFrom(src => src.score.ToString(CultureInfo.CurrentUICulture)))
                .ForMember(dest => dest.Runtime, opts => opts.MapFrom(src => src.show.runtime.ToString()))
                .ForMember(dest => dest.SeriesId, opts => opts.MapFrom(src => src.show.id))
                .ForMember(dest => dest.SeriesName, opts => opts.MapFrom(src => src.show.name))
                .ForMember(dest => dest.Banner, opts => opts.MapFrom(src => !string.IsNullOrEmpty(src.show.image.medium) ? src.show.image.medium.Replace("http","https") : string.Empty))
                .ForMember(dest => dest.Status, opts => opts.MapFrom(src => src.show.status));
        }
    }
}