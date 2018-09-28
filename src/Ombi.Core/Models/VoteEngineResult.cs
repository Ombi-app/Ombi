namespace Ombi.Core.Models
{
    public class VoteEngineResult
    {
        public bool Result { get; set; }
        public string Message { get; set; }
        public bool IsError => !string.IsNullOrEmpty(ErrorMessage);
        public string ErrorMessage { get; set; }
    }
}