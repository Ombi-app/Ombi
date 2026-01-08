using System.Threading.Tasks;
using Ombi.Api.External.NotificationServices.Slack.Models;

namespace Ombi.Api.External.NotificationServices.Slack
{
    public interface ISlackApi
    {
        Task<string> PushAsync(string team, string token, string service, SlackNotificationBody message);
    }
}