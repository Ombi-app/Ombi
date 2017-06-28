namespace Ombi.Core.Models.Requests
{
    public enum IssueState
    {
        None = 99,
        WrongAudio = 0,
        NoSubtitles = 1,
        WrongContent = 2,
        PlaybackIssues = 3,
        Other = 4 // Provide a message
    }
}