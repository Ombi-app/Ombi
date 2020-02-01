using System.Threading.Tasks;

namespace Ombi.Api.Twilio
{
    public interface IWhatsAppApi
    {
        Task<string> SendMessage(WhatsAppModel message, string accountSid, string authToken);
    }
}