using System;
using System.Collections.Generic;
using Ombi.Store.Entities;

namespace Ombi.Core.Models.Requests
{
    public class TvRequestModel : BaseRequestModel
    {
        public TvRequestModel()
        {
            ChildRequests = new List<ChildTvRequest>();
        }
        public string ImdbId { get; set; }
        public string TvDbId { get; set; }

        public List<ChildTvRequest> ChildRequests { get; set; }
        
        /// <summary>
        /// For TV Shows with a custom root folder
        /// </summary>
        /// <value>
        /// The root folder selected.
        /// </value>
        public int RootFolderSelected { get; set; }
    }

    public class ChildTvRequest : BaseRequestModel
    {
        public bool RequestAll { get; set; }
        public List<SeasonRequestModel> SeasonRequests { get; set; } = new List<SeasonRequestModel>();

    }
}