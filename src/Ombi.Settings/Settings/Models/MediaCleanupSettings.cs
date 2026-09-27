using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ombi.Store.Entities;

namespace Ombi.Settings.Settings.Models
{
    public class MediaCleanupSettings : Settings
    {
        [EnumDataType(typeof(OwnRequestRemovalMode))]
        public OwnRequestRemovalMode OwnRequestRemoval { get; set; } = OwnRequestRemovalMode.Off;

        [EnumDataType(typeof(CommunityCleanupMode))]
        public CommunityCleanupMode CommunityCleanup { get; set; } = CommunityCleanupMode.Off;

        public int MinimumDeleteVotes { get; set; } = 3;
        public int RequiredVoteMargin { get; set; } = 1;
        public int VotingPeriodDays { get; set; } = 7;
        public int GracePeriodDays { get; set; } = 3;
        public int MinimumMediaAgeDays { get; set; } = 30;
        public bool AnyKeepVotePreventsRemoval { get; set; } = false;
        public bool RequesterCanVeto { get; set; } = true;
        public bool RestrictNominationsToOwnRequests { get; set; } = false;
        public bool DeleteFiles { get; set; } = true;
        public bool AddImportExclusion { get; set; }
        public bool NotifyManagersOnPendingApproval { get; set; } = true;
        public bool NotifyVotersOnPendingVotes { get; set; }

        public bool Enabled => OwnRequestRemoval != OwnRequestRemovalMode.Off || CommunityCleanup != CommunityCleanupMode.Off;
    }

    public enum OwnRequestRemovalMode
    {
        Off = 0,
        RequestRemoval = 1,
        ImmediateDeletion = 2
    }

    public enum CommunityCleanupMode
    {
        Off = 0,
        AdminApproval = 1,
        AutomaticAfterThreshold = 2
    }

    public enum MediaCleanupStatus
    {
        Voting = 0,
        PendingAdminApproval = 1,
        ScheduledForDeletion = 2,
        Completed = 3,
        Rejected = 4,
        Failed = 5,
        Cancelled = 6
    }

    public enum MediaCleanupOrigin
    {
        OwnRequest = 0,
        Community = 1
    }

    public enum MediaCleanupVoteType
    {
        Keep = 0,
        Delete = 1
    }

    /// <summary>
    /// Persistent cleanup workflow state. This intentionally lives in the settings store so the
    /// feature can be applied to existing Ombi installations without a provider-specific schema
    /// migration. A future upstream implementation can move these records to first-class tables.
    /// </summary>
    public class MediaCleanupState : Settings
    {
        public List<MediaCleanupRecord> Requests { get; set; } = new List<MediaCleanupRecord>();
    }

    public class MediaCleanupRecord
    {
        public string Id { get; set; }
        public RequestType RequestType { get; set; }
        public int MediaRequestId { get; set; }
        public string Title { get; set; }
        public string PosterPath { get; set; }
        public int TheMovieDbId { get; set; }
        public int TvDbId { get; set; }
        public DateTime? AvailableSince { get; set; }
        public long SizeOnDisk { get; set; }
        public string RequestedByUserId { get; set; }
        public List<string> OwnerUserIds { get; set; } = new List<string>();
        public MediaCleanupOrigin Origin { get; set; }
        public MediaCleanupStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? VotingEndsAt { get; set; }
        public DateTime? ScheduledForDeletionAt { get; set; }
        /// <summary>
        /// Set immediately after Radarr/Sonarr confirms the destructive operation, before
        /// Ombi request/cache reconciliation begins. A populated value makes reconciliation
        /// retryable without issuing the destructive external operation again.
        /// </summary>
        public DateTime? ExternalDeletionCompletedAt { get; set; }
        /// <summary>
        /// Consecutive transient failures in the current deletion/reconciliation phase.
        /// Reset when a phase succeeds. Existing serialized records default to zero.
        /// </summary>
        public int RetryCount { get; set; }
        public DateTime? LastFailureAt { get; set; }
        public DateTime? NextRetryAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string ApprovedByUserId { get; set; }
        public string FailureReason { get; set; }
        /// <summary>
        /// Frozen destructive behavior and external destination identity captured when the
        /// cleanup becomes authorized for deletion. API keys are deliberately not stored.
        /// </summary>
        public MediaCleanupDeletionPlan DeletionPlan { get; set; }
        /// <summary>
        /// Empty means the cleanup targets the entire TV series. Populated entries make the
        /// cleanup episode-scoped while keeping old serialized records backwards compatible.
        /// </summary>
        public List<MediaCleanupEpisodeRecord> SelectedEpisodes { get; set; } = new List<MediaCleanupEpisodeRecord>();
        public List<int> SelectedSeasons { get; set; } = new List<int>();
        public List<MediaCleanupVoteRecord> Votes { get; set; } = new List<MediaCleanupVoteRecord>();
    }

    public enum MediaCleanupExternalService
    {
        Radarr = 0,
        Radarr4K = 1,
        Sonarr = 2
    }

    public class MediaCleanupDeletionPlan
    {
        public int Version { get; set; } = 1;
        public bool DeleteFiles { get; set; }
        public bool AddImportExclusion { get; set; }
        public DateTime AuthorizedAt { get; set; }
        public List<MediaCleanupExternalTarget> Targets { get; set; } = new List<MediaCleanupExternalTarget>();
    }

    public class MediaCleanupExternalTarget
    {
        public MediaCleanupExternalService Service { get; set; }
        public string Endpoint { get; set; }
    }

    public class MediaCleanupEpisodeRecord
    {
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public string Title { get; set; }
        public int EpisodeFileId { get; set; }
        public long SizeOnDisk { get; set; }
    }

    public class MediaCleanupVoteRecord
    {
        public string UserId { get; set; }
        public MediaCleanupVoteType Vote { get; set; }
        public DateTime Date { get; set; }
    }
}
