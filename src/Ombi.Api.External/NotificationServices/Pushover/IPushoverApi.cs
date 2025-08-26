using System.Threading.Tasks;
using Ombi.Api.External.NotificationServices.Pushover.Models;

namespace Ombi.Api.External.NotificationServices.Pushover
{
    public interface IPushoverApi
    {
        Task<PushoverResponse> PushAsync(string accessToken, string message, string userToken, sbyte priority, string sound);
    }
}