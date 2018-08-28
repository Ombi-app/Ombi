using System.Threading.Tasks;
using Ombi.Api.Pushover.Models;

namespace Ombi.Api.Pushover
{
    public interface IPushoverApi
    {
        Task<PushoverResponse> PushAsync(string accessToken, string message, string userToken, sbyte priority, string sound);
    }
}