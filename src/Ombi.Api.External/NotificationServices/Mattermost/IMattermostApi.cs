using System.Threading.Tasks;
using Ombi.Api.External.NotificationServices.Mattermost.Models;

namespace Ombi.Api.External.NotificationServices.Mattermost
{
    public interface IMattermostApi
    {
        Task PushAsync(string webhook, MattermostMessage message);
    }
}