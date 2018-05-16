using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities.Requests
{
    [Table("MovieRequests")]
    public class MovieRequests : FullBaseRequest
    {
        public int TheMovieDbId { get; set; }
        public int? IssueId { get; set; }
        [ForeignKey(nameof(IssueId))]
        public List<Issues> Issues { get; set; }

        [NotMapped]
        public bool Subscribed { get; set; }

        public int RootPathOverride { get; set; }
        public int QualityOverride { get; set; }
    }
}
