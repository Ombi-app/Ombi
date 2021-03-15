using System;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Ombi.Api.TheMovieDb.Models;
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
                //.ForMember(dest => dest.Images, opts => opts.MapFrom(src => src.image))
                .ForMember(dest => dest.Cast, opts => opts.MapFrom(src => src._embedded.cast))
                .ForMember(dest => dest.Crew, opts => opts.MapFrom(src => src._embedded.crew))
                .ForMember(dest => dest.Banner,
                    opts => opts.MapFrom(src =>
                        !string.IsNullOrEmpty(src.image.medium)
                            ? src.image.medium.ToHttpsUrl()
                            : string.Empty))
                .ForMember(dest => dest.Status, opts => opts.MapFrom(src => src.status));



            CreateMap<TvInfo, SearchFullInfoTvShowViewModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.id))
                .ForMember(dest => dest.FirstAired, opts => opts.MapFrom(src => src.first_air_date))
                .ForMember(dest => dest.ImdbId, opts => opts.MapFrom(src => src.ExternalIds.ImdbId))
                .ForMember(dest => dest.TheTvDbId, opts => opts.MapFrom(src => src.ExternalIds.TvDbId))
                .ForMember(dest => dest.Network, opts => opts.MapFrom(src => src.networks.FirstOrDefault()))
                .ForMember(dest => dest.NetworkId, opts => opts.MapFrom(src => src.networks.FirstOrDefault().id))
                .ForMember(dest => dest.Overview, opts => opts.MapFrom(src => src.overview))
                .ForMember(dest => dest.Rating,
                    opts => opts.MapFrom(src => src.vote_average.ToString(CultureInfo.CurrentUICulture)))
                //.ForMember(dest => dest.Runtime, opts => opts.MapFrom(src => src.runtime.ToString()))
                .ForMember(dest => dest.SeriesId, opts => opts.MapFrom(src => src.id))
                .ForMember(dest => dest.Title, opts => opts.MapFrom(src => src.name))
                //.ForMember(dest => dest.Network, opts => opts.MapFrom(src => src.network))
                .ForMember(dest => dest.Images, opts => opts.MapFrom(src => src.Images))
                .ForMember(dest => dest.Cast, opts => opts.MapFrom(src => src.Credits.cast))
                .ForMember(dest => dest.Crew, opts => opts.MapFrom(src => src.Credits.crew))
                .ForMember(dest => dest.Banner, opts => opts.MapFrom(src => GetBanner(src.Images)))
                .ForMember(dest => dest.Genres, opts => opts.MapFrom(src => src.genres))
                .ForMember(dest => dest.Keywords, opts => opts.MapFrom(src => src.Keywords))
                .ForMember(dest => dest.Tagline, opts => opts.MapFrom(src => src.tagline))
                .ForMember(dest => dest.ExternalIds, opts => opts.MapFrom(src => src.ExternalIds))
                .ForMember(dest => dest.Trailer, opts => opts.MapFrom(src => GetTrailer(src.Videos)))
                .ForMember(dest => dest.Runtime, opts => opts.MapFrom(src => src.episode_run_time.FirstOrDefault()))
                .ForMember(dest => dest.Status, opts => opts.MapFrom(src => src.status));

            CreateMap<Ombi.Api.TheMovieDb.Models.ExternalIds, Ombi.Core.Models.Search.V2.ExternalIds>().ReverseMap();
            CreateMap<Ombi.Api.TheMovieDb.Models.Images, Ombi.Core.Models.Search.V2.Images>()
                .ForMember(dest => dest.Original, opts => opts.MapFrom(src => src.Posters.OrderBy(x => x.VoteCount).ThenBy(x => x.VoteAverage).FirstOrDefault().FilePath));


            CreateMap<Api.TheMovieDb.Models.Keywords, Ombi.Core.Models.Search.V2.Keywords>().ReverseMap();
            CreateMap<Ombi.Api.TheMovieDb.Models.KeywordsValue, Ombi.Core.Models.Search.V2.KeywordsValue>().ReverseMap();


            CreateMap<GenreViewModel, Genre>()
                .ForMember(dest => dest.id, opts => opts.MapFrom(src => src.id))
                .ForMember(dest => dest.name, opts => opts.MapFrom(src => src.name));

            CreateMap<Api.TvMaze.Models.V2.Network, NetworkViewModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.id))
                .ForMember(dest => dest.Country, opts => opts.MapFrom(src => src.country))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.name));

            CreateMap<Api.TheMovieDb.Models.Network, NetworkViewModel>()
    .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.id))
    .ForMember(dest => dest.Country, opts => opts.MapFrom(src => src.origin_country))
    .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.name));

            CreateMap<Api.TvMaze.Models.V2.Country, Core.Models.Search.V2.Country>()
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.name))
                .ForMember(dest => dest.Code, opts => opts.MapFrom(src => src.code))
                .ForMember(dest => dest.Timezone, opts => opts.MapFrom(src => src.timezone));

            //CreateMap<Api.TvMaze.Models.V2.Image, Images>()
            //    .ForMember(dest => dest.Medium, opts => opts.MapFrom(src => src.medium.ToHttpsUrl()))
            //    .ForMember(dest => dest.Original, opts => opts.MapFrom(src => src.original.ToHttpsUrl()));

            //CreateMap<Api.TvMaze.Models.V2.Cast, CastViewModel>()
            //    .ForMember(dest => dest.Character, opts => opts.MapFrom(src => src.character))
            //    .ForMember(dest => dest.Person, opts => opts.MapFrom(src => src.person))
            //    .ForMember(dest => dest.Voice, opts => opts.MapFrom(src => src.voice))
            //    .ForMember(dest => dest.Self, opts => opts.MapFrom(src => src.self));

            CreateMap<FullMovieCast, CastViewModel>()
    .ForMember(dest => dest.Character, opts => opts.MapFrom(src => src.character))
    .ForMember(dest => dest.Person, opts => opts.MapFrom(src => src.name))
    .ForMember(dest => dest.Image, opts => opts.MapFrom(src => src.profile_path))
    .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.id));

            CreateMap<Api.TvMaze.Models.V2.Person, PersonViewModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.id))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.name))
                .ForMember(dest => dest.Image, opts => opts.MapFrom(src => src.image))
                .ForMember(dest => dest.Url, opts => opts.MapFrom(src => src.url.ToHttpsUrl()));

            //CreateMap<Api.TvMaze.Models.V2.Crew, CrewViewModel>()
            //    .ForMember(dest => dest.Person, opts => opts.MapFrom(src => src.person))
            //    .ForMember(dest => dest.Type, opts => opts.MapFrom(src => src.type));

            CreateMap<FullMovieCrew, PersonViewModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.id))
                .ForMember(dest => dest.Image, opts => opts.MapFrom(src => src.profile_path))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.name));

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

        private string GetBanner(Api.TheMovieDb.Models.Images images)
        {
            var hasBackdrop = images?.Backdrops?.Any();
            if (hasBackdrop ?? false)
            {
                return images.Backdrops?.OrderBy(x => x.VoteCount).ThenBy(x => x.VoteAverage).Select(x => x.FilePath).FirstOrDefault();
            }
            else if (images != null)
            {
                return images.Posters?.OrderBy(x => x.VoteCount).ThenBy(x => x.VoteAverage).Select(x => x.FilePath).FirstOrDefault();
            }
            else
            {
                return string.Empty;
            }
        }

        private string GetTrailer(Api.TheMovieDb.Models.Videos trailer)
        {
            return trailer?.results?.FirstOrDefault(x => x.type.Equals("trailer", StringComparison.InvariantCultureIgnoreCase))?.key ?? null;
        }
    }
}