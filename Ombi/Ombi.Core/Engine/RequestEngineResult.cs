namespace Ombi.Core.Engine
{
    public class RequestEngineResult
    {
        public bool RequestAdded { get; set; }
        public string Message { get; set; }
        public bool IsError => !string.IsNullOrEmpty(ErrorMessage);
        public string ErrorMessage { get; set; }
    }
}