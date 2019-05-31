using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Ombi.Api.GroupMe.Models;

namespace Ombi.Api.GroupMe
{
    public class GroupMeApi : IGroupMeApi
    {
        public GroupMeApi(IApi api)
        {
            _api = api;
        }

        private readonly IApi _api;
        private const string BaseUrl = "https://api.groupme.com/v3/";


        public async Task<GroupMeResponse<List<Groups>>> GetGroups(string token, CancellationToken cancellationToken)
        {
            var request = new Request($"groups", BaseUrl, HttpMethod.Get);
            request.AddQueryString("omit", "memberships");

            AddHeaders(request, token);

            return await _api.Request<GroupMeResponse<List<Groups>>>(request, cancellationToken);
        } 

        public async Task<GroupMeResponse<SendResponse>> Send(string message, string token, int groupId)
        {
            var request = new Request($"groups/{groupId}/messages", BaseUrl, HttpMethod.Post);

            AddHeaders(request, token);

            var body = new
            {
                message = new
                {
                    source_guid = Guid.NewGuid(),
                    text = message
                }
            };

            request.AddJsonBody(body);
            return await _api.Request<GroupMeResponse<SendResponse>>(request);
        }

        private void AddHeaders(Request req, string token)
        {
            req.AddHeader("X-Access-Token", token);
        }
    }
}
