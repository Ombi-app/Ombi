using Ombi.Store.Entities;
using System;

namespace Ombi.Core.Models.Requests
{
    public class RequestViewModel
    {
        public int Id { get; set; }
        public int ProviderId { get; set; }
        public string ImdbId { get; set; }
        public string Overview { get; set; }
        public string Title { get; set; }
        public string PosterPath { get; set; }
        public DateTime ReleaseDate { get; set; }
        public bool Released { get; set; }
        public RequestType Type { get; set; }
        public string Status { get; set; }
        public bool Approved { get; set; }
        public string[] RequestedUsers { get; set; }
        public DateTime RequestedDate { get; set; }
        public string ReleaseYear { get; set; }
        public bool Available { get; set; }
        public bool Admin { get; set; }
        public int IssueId { get; set; }
        public QualityModel[] Qualities { get; set; }
        public EpisodesModel[] Episodes { get; set; }
        public bool Denied { get; set; }
        public string DeniedReason { get; set; }
        public RootFolderModel[] RootFolders { get; set; }
        public bool HasRootFolders { get; set; }
        public string CurrentRootPath { get; set; }
    }
}