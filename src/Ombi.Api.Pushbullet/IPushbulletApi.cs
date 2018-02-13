using System.Threading.Tasks;

namespace Ombi.Api.Pushbullet
{
    public interface IPushbulletApi
    {
        Task Push(string accessToken, string subject, string body, string channelTag);
    }
}