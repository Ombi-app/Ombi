using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ombi.Api.GroupMe.Models;

namespace Ombi.Api.GroupMe
{
    public interface IGroupMeApi
    {
        Task<GroupMeResponse<List<Groups>>> GetGroups(string token, CancellationToken cancellationToken);
        Task<GroupMeResponse<SendResponse>> Send(string message, string token, int groupId);
    }
}