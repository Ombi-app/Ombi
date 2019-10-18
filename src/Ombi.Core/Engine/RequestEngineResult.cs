namespace Ombi.Core.Engine
{
    public class RequestEngineResult
    {
        public bool Result { get; set; }
        public string Message { get; set; }
        public bool IsError => !string.IsNullOrEmpty(ErrorMessage);
        public string ErrorMessage { get; set; }
        public int RequestId { get; set; }
    }
}