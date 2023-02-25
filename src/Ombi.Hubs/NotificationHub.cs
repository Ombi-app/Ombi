using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Authentication;
using Ombi.Store.Entities;

namespace Ombi.Hubs;

public class NotificationHub : Hub
{
    private readonly OmbiUserManager _userManager;
    public static readonly ConcurrentDictionary<string, NotificationHubUser> UsersOnline = new();
    
    public NotificationHub(OmbiUserManager userManager)
    {
        _userManager = userManager;
    }

    public override async Task OnConnectedAsync()
    {
        ClaimsIdentity identity = (ClaimsIdentity)Context.User?.Identity;
        Claim userIdClaim = identity?.Claims
            .FirstOrDefault(x => x.Type.Equals("Id", StringComparison.InvariantCultureIgnoreCase));
        if (userIdClaim == null)
        {
            await base.OnConnectedAsync();
            return;
        }

        OmbiUser user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userIdClaim.Value);
        IList<string> claims = await _userManager.GetRolesAsync(user);
        UsersOnline.TryAdd(Context.ConnectionId, new NotificationHubUser
        {
            UserId = userIdClaim.Value,
            Roles = claims
        });
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        UsersOnline.TryRemove(Context.ConnectionId, out _);
        await base.OnDisconnectedAsync(exception);
    }
}