using System.Threading.Tasks;

namespace Ombi.Api.Telegram
{
    public interface ITelegramRequestAnswer
    {
        Task<string> ApproveMovie(int id);
        Task<string> DenyMovie(int id);
        Task<string> ApproveTv(int id);
        Task<string> DenyTv(int id);
    }
}
