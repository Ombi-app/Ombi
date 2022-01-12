﻿using Ombi.Store.Repository.Requests;
using System.Collections.Generic;
using Ombi.Store.Entities;

namespace Ombi.Core.Models.Search.V2
{
    public class SearchFullInfoTvShowViewModel : SearchViewModel
    {
        public string Title { get; set; }
        public List<string> Aliases { get; set; }
        public string Banner { get; set; }
        public int SeriesId { get; set; }
        public string Status { get; set; }
        public string FirstAired { get; set; }
        public string NetworkId { get; set; }
        public string Runtime { get; set; }
        public GenreViewModel[] Genres { get; set; }
        public string Overview { get; set; }
        public int LastUpdated { get; set; }
        public string AirsDayOfWeek { get; set; }
        public string AirsTime { get; set; }
        public string Rating { get; set; }
        public int SiteRating { get; set; }
        public NetworkViewModel Network { get; set; }
        public Images Images { get; set; }
        public List<CastViewModel> Cast { get; set; }
        public List<PersonViewModel> Crew { get; set; }
        public string Certification { get; set; }
        public string Tagline { get; set; }
        public Keywords Keywords { get; set; }
        public ExternalIds ExternalIds { get; set; }
        public string Trailer { get; set; }
        public string Homepage { get; set; }

        public List<SeasonRequests> SeasonRequests { get; set; } = new List<SeasonRequests>();

        /// <summary>
        ///     If we are requesting the entire series
        /// </summary>
        public bool RequestAll { get; set; }

        public bool FirstSeason { get; set; }
        public bool LatestSeason { get; set; }

        /// <summary>
        /// This is where we have EVERY Episode for that series
        /// </summary>
        public bool FullyAvailable { get; set; }
        // We only have some episodes
        public bool PartlyAvailable { get; set; }
        public override RequestType Type => RequestType.TvShow;
    }

    public class NetworkViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
    }

    public class Country
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Timezone { get; set; }
    }

    public class Images
    {
        public string Medium { get; set; }
        public string Original { get; set; }
    }

    public class CastViewModel
    {
        public int Id { get; set; }
        public string Person { get; set; }
        public string Character { get; set; }
        public string Image { get; set; }
        public bool Self { get; set; }
        public bool Voice { get; set; }
    }

    public class PersonViewModel
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
    }

    public class CharacterViewModel
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public Images Image { get; set; }
    }

}