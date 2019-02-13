using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Ombi.Hubs
{
    public class ScheduledJobsHub : Hub
    {
        public Task Send(string data)
        {
            return Clients.All.SendAsync("Send", data);
        }
    }
}
