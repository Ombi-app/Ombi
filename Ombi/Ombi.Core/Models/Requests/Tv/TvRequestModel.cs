using System.Collections.Generic;

namespace Ombi.Core.Models.Requests
{
    public class TvRequestModel : BaseRequestModel
    {
        public TvRequestModel()
        {
            Episodes = new List<EpisodesModel>();
            ChildRequests = new List<TvRequestModel>();
        }
        
        public string ImdbId { get; set; }
        public string TvDbId { get; set; }
       public bool RequestAll { get; set; }
        public List<int> SeasonsNumbersRequested { get; set; }
        public List<EpisodesModel> Episodes { get; set; }

        /// <summary>
        /// This is for TV requests, If there is more than 1 request for a show then it should be a child
        /// e.g. Request 1 is for Season 1, Request 2 is for season 5. There should be two child items.
        /// </summary>
        public List<TvRequestModel> ChildRequests { get; set; }

        public bool HasChildRequests => ChildRequests.Count > 0;

        /// <summary>
        /// For TV Shows with a custom root folder
        /// </summary>
        /// <value>
        /// The root folder selected.
        /// </value>
        public int RootFolderSelected { get; set; }
    }
}