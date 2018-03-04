using System.Threading.Tasks;
using Ombi.Core.Models;

namespace Ombi.Core.Senders
{
    public interface IMassEmailSender
    {
        Task<bool> SendMassEmail(MassEmailModel model);
    }
}