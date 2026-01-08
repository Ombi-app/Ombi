using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ombi.Api.External.NotificationServices.GroupMe.Models;

namespace Ombi.Api.External.NotificationServices.GroupMe
{
    public interface IGroupMeApi
    {
        Task<GroupMeResponse<List<Groups>>> GetGroups(string token, CancellationToken cancellationToken);
        Task<GroupMeResponse<SendResponse>> Send(string message, string token, int groupId);
    }
}