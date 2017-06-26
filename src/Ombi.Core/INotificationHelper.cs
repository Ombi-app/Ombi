using Ombi.Core.Models.Requests;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core
{
    public interface INotificationHelper
    {
        void NewRequest(FullBaseRequest model);
    }
}