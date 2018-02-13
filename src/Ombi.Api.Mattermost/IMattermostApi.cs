using System.Threading.Tasks;
using Ombi.Api.Mattermost.Models;

namespace Ombi.Api.Mattermost
{
    public interface IMattermostApi
    {
        Task<string> PushAsync(string webhook, MattermostBody message);
    }
}