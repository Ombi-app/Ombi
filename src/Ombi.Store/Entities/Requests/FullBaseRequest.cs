using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities.Requests
{
    public class FullBaseRequest : BaseRequest
    {
        public string ImdbId { get; set; }
        public string Overview { get; set; }
        public string Title { get; set; }
        public string PosterPath { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Status { get; set; }


        [NotMapped]
        public bool Released => DateTime.UtcNow > ReleaseDate;
    }
}