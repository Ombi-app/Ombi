namespace Ombi.Api.Radarr.Models
{
    public class RadarrError
    {
        public string message { get; set; }
        public string description { get; set; }
    }

    public class RadarrErrorResponse
    {
        public string propertyName { get; set; }
        public string errorMessage { get; set; }
        public object attemptedValue { get; set; }
        public FormattedMessagePlaceholderValues formattedMessagePlaceholderValues { get; set; }
    }
    public class FormattedMessagePlaceholderValues
    {
        public string propertyName { get; set; }
        public object propertyValue { get; set; }
    }
}