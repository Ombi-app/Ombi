namespace Ombi.Api.External.NotificationServices.Twilio
{
    public class WhatsAppModel
    {
        public string Message { get; set; }
        public string To { get; set; }
        public string From { get; set; }
    }
}