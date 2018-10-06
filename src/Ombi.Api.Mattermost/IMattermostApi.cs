using System.Threading.Tasks;
using Matterhook.NET.MatterhookClient;

namespace Ombi.Api.Mattermost
{
    public interface IMattermostApi
    {
        Task PushAsync(string webhook, MattermostMessage message);
    }
}