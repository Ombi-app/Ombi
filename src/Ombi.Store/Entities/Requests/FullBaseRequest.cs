using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities.Requests
{
    public class FullBaseRequest : BaseRequest
    {
        public string ImdbId { get; set; }
        public string Overview { get; set; }
        public string PosterPath { get; set; }
        public DateTime ReleaseDate { get; set; }
        public DateTime? DigitalReleaseDate { get; set; }
        public string Status { get; set; }
        public string Background { get; set; }


        [NotMapped]
        public bool Released => DateTime.UtcNow > ReleaseDate;
        [NotMapped]
        public bool DigitalRelease => DigitalReleaseDate.HasValue && DigitalReleaseDate > DateTime.UtcNow;
    }
}