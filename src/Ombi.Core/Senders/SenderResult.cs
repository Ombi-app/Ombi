namespace Ombi.Core.Senders
{
    public class SenderResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool Sent { get; set; }
    }
}