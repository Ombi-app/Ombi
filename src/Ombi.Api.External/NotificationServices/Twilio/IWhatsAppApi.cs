using System.Threading.Tasks;

namespace Ombi.Api.External.NotificationServices.Twilio
{
    public interface IWhatsAppApi
    {
        Task<string> SendMessage(WhatsAppModel message, string accountSid, string authToken);
    }
}