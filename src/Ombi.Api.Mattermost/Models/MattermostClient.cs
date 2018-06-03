///
/// 
/// 
/// Code taken from https://github.com/PromoFaux/Matterhook.NET.MatterhookClient
/// 
/// 
/// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Ombi.Api.Mattermost.Models
{
    public class MatterhookClient
    {
        private readonly Uri _webhookUrl;
        private readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Create a new Mattermost Client
        /// </summary>
        /// <param name="webhookUrl">The URL of your Mattermost Webhook</param>
        /// <param name="timeoutSeconds">Timeout Value (Default 100)</param>
        public MatterhookClient(string webhookUrl, int timeoutSeconds = 100)
        {
            if (!Uri.TryCreate(webhookUrl, UriKind.Absolute, out _webhookUrl))
                throw new ArgumentException("Mattermost URL invalid");

            _httpClient.Timeout = new TimeSpan(0, 0, 0, timeoutSeconds);
        }

        public MattermostMessage CloneMessage(MattermostMessage inMsg)
        {
            var outMsg = new MattermostMessage
            {
                Text = "",
                Channel = inMsg.Channel,
                Username = inMsg.Username,
                IconUrl = inMsg.IconUrl
            };

            return outMsg;
        }

        private static MattermostAttachment CloneAttachment(MattermostAttachment inAtt)
        {
            var outAtt = new MattermostAttachment
            {
                AuthorIcon = inAtt.AuthorIcon,
                AuthorLink = inAtt.AuthorLink,
                AuthorName = inAtt.AuthorName,
                Color = inAtt.Color,
                Fallback = inAtt.Fallback,
                Fields = inAtt.Fields,
                ImageUrl = inAtt.ImageUrl,
                Pretext = inAtt.Pretext,
                ThumbUrl = inAtt.ThumbUrl,
                Title = inAtt.Title,
                TitleLink = inAtt.TitleLink,
                Text = ""
            };
            return outAtt;
        }

        /// <summary>
        /// Post Message to Mattermost server. Messages will be automatically split if total text length > 4000
        /// </summary>
        /// <param name="api"></param>
        /// <param name="inMessage">The messsage you wish to send</param>
        /// <returns></returns>
        public async Task PostAsync(IApi api, MattermostMessage inMessage)
        {
            try
            {
                var outMessages = new List<MattermostMessage>();

                var msgCount = 0;

                var lines = new string[] { };
                if (inMessage.Text != null)
                {
                    lines = inMessage.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                }

                //start with one cloned inMessage in the list
                outMessages.Add(CloneMessage(inMessage));

                //add text from original. If we go over 3800, we'll split it to a new inMessage.
                foreach (var line in lines)
                {

                    if (line.Length + outMessages[msgCount].Text.Length > 3800)
                    {

                        msgCount += 1;
                        outMessages.Add(CloneMessage(inMessage));
                    }

                    outMessages[msgCount].Text += $"{line}\r\n";
                }

                //Length of text on the last (or first if only one) inMessage.
                var lenMessageText = outMessages[msgCount].Text.Length;

                //does our original have attachments?
                if (inMessage.Attachments?.Any() ?? false)
                {
                    outMessages[msgCount].Attachments = new List<MattermostAttachment>();

                    //loop through them in a similar fashion to the inMessage text above.
                    foreach (var att in inMessage.Attachments)
                    {
                        //add this attachment to the outgoing message
                        outMessages[msgCount].Attachments.Add(CloneAttachment(att));
                        //get a count of attachments on this message, and subtract one so we know the index of the current new attachment
                        var attIndex = outMessages[msgCount].Attachments.Count - 1;

                        //Get the text lines
                        if (!String.IsNullOrEmpty(att.Text))
                        {
                            lines = att.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                        }
                        foreach (var line in lines)
                        {
                            //Get the total length of all attachments on the current outgoing message
                            var lenAllAttsText = outMessages[msgCount].Attachments.Sum(a => a.Text.Length);

                            if (lenMessageText + lenAllAttsText + line.Length > 3800)
                            {
                                msgCount += 1;
                                attIndex = 0;
                                outMessages.Add(CloneMessage(inMessage));
                                outMessages[msgCount].Attachments = new List<MattermostAttachment> { CloneAttachment(att) };
                            }

                            outMessages[msgCount].Attachments[attIndex].Text += $"{line}\r\n";
                        }
                    }
                }


                if (outMessages.Count > 1)
                {
                    var num = 1;
                    foreach (var msg in outMessages)
                    {
                        msg.Text = $"`({num}/{msgCount + 1}): ` " + msg.Text;
                        num++;
                    }
                }

                foreach (var msg in outMessages)
                {
                    var request = new Request(_webhookUrl.ToString(), "", HttpMethod.Post);
                    request.AddJsonBody(msg);
                    request.AddHeader("Host", _webhookUrl.Host);
                    await api.Request(request);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }
}