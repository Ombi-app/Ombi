using System.Threading.Tasks;
using Ombi.Api.Mattermost.Models;

namespace Ombi.Api.Mattermost
{
    public interface IMattermostApi
    {
        Task PushAsync(string webhook, MattermostMessage message);
    }
}