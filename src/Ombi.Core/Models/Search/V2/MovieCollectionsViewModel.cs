using System;
using System.Collections.Generic;
using Ombi.Store.Entities;

namespace Ombi.Core.Models.Search.V2
{
    public class MovieCollectionsViewModel
    {
        public string Name { get; set; }
        public string Overview { get; set; }
        public List<MovieCollection> Collection { get; set; }
    }

    public class MovieCollection : SearchViewModel
    {
        public string Overview { get; set; }
        public string PosterPath { get; set; }
        public string Title { get; set; }
        public DateTime ReleaseDate { get; set; }


        public override RequestType Type => RequestType.Movie;
    }
}