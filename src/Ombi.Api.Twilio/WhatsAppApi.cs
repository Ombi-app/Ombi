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
            // Find your Account Sid and Token at twilio.com/console
            // DANGER! This is insecure. See http://twil.io/secure
            //const string accountSid = "AC8a1b6ab0d9f351be8210ccc8f7930d27";
            //const string authToken = "f280272092780a770f7cd4fb0beed125";

            TwilioClient.Init(accountSid, authToken);

            var response =await  MessageResource.CreateAsync(
                body: message.Message,
                from: new PhoneNumber($"whatsapp:{message.From}"),
                to: new PhoneNumber($"whatsapp:{message.To}")
            );

            return response.Sid;
        }
    }
}
