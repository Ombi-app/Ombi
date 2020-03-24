using System.Threading.Tasks;
using Ombi.Api.Emby.Models;

namespace Ombi.Api.Emby
{
    public interface IEmbyApi : IBaseEmbyApi
    {      
        Task<EmbyConnectUser> LoginConnectUser(string username, string password);
    }
}