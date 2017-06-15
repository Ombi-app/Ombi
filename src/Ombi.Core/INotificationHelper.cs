using Ombi.Core.Models.Requests;

namespace Ombi.Core
{
    public interface INotificationHelper
    {
        void NewRequest(BaseRequestModel model);
    }
}