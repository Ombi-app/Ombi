using System.Threading.Tasks;

namespace Ombi.Api.External.NotificationServices.Pushbullet
{
    public interface IPushbulletApi
    {
        Task Push(string accessToken, string subject, string body, string channelTag);
    }
}