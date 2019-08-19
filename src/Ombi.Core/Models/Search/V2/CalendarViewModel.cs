using System;
using System.Collections.Generic;
using Ombi.Store.Entities;

namespace Ombi.Core.Models.Search.V2
{
    public class CalendarViewModel
    {
        public string Title { get; set; }
        public DateTime Start { get; set; }
        public string BackgroundColor { get; set; }
        public RequestType Type { get; set; }
        public List<ExtraParams> ExtraParams { get; set; }

        public string BorderColor
        {
            get
            {
                switch (Type)
                {
                    case RequestType.TvShow:
                        return "#ff0000";
                    case RequestType.Movie:
                        return "#0d5a3e";
                    case RequestType.Album:
                        return "#797979";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    public class ExtraParams
    {
        public int ProviderId { get; set; }
        public string Overview { get; set; }
        public RequestType Type { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string RequestStatus { get; set; }
    }
}