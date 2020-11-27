using System;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Ombi.Api.Twilio
{
    public class WhatsAppApi : IWhatsAppApi
    {
        public async Task<string> SendMessage(WhatsAppModel message, string accountSid, string authToken)
        {
            TwilioClient.Init(accountSid, authToken);

            if(string.IsNullOrEmpty(message.To))
            {
                return string.Empty;
            }
            var response =await  MessageResource.CreateAsync(
                body: message.Message,
                from: new PhoneNumber($"whatsapp:{message.From}"),
                to: new PhoneNumber($"whatsapp:{message.To}")
            );

            return response.Sid;
        }
    }
}
