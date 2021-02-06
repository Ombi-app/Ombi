using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;

namespace Ombi.Store.Repository.Requests
{

    public class SeasonRequests : Entity
    {
        public int SeasonNumber { get; set; }
        public List<EpisodeRequests> Episodes { get; set; } = new List<EpisodeRequests>();
        
        public int ChildRequestId { get; set; }
        [ForeignKey(nameof(ChildRequestId))]
        public ChildRequests ChildRequest { get; set; }
        [NotMapped] public bool SeasonAvailable { get; set; }
    }

    public class EpisodeRequests : Entity
    {
        public int EpisodeNumber { get; set; }
        public string Title { get; set; }
        public DateTime AirDate { get; set; }
        public string Url { get; set; }
        public bool Available { get; set; }
        public bool Approved { get; set; }
        public bool Requested { get; set; }

        public int SeasonId { get; set; }
        [ForeignKey(nameof(SeasonId))]
        public SeasonRequests Season { get; set; }

        [NotMapped] public string AirDateDisplay => AirDate == DateTime.MinValue ? "Unknown" : AirDate.ToString(CultureInfo.InvariantCulture);

        [NotMapped]
        public string RequestStatus
        {
            get
            {
                if (Available)
                {
                    return "Common.Available";
                }

                if (Approved & !Available)
                {
                    return "Common.ProcessingRequest";
                }

                if (!Approved && !Available && Requested)
                {
                    return "Common.PendingApproval";
                }

                return string.Empty;
            }
        }
    }
}