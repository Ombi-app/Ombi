using System.Threading.Tasks;
using Ombi.Api.Slack.Models;

namespace Ombi.Api.Slack
{
    public interface ISlackApi
    {
        Task<string> PushAsync(string team, string token, string service, SlackNotificationBody message);
    }
}