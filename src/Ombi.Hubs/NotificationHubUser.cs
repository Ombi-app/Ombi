using System.Collections.Generic;

namespace Ombi.Hubs;

public class NotificationHubUser
{
    public string UserId { get; set; }
    public IList<string> Roles { get; init; } = new List<string>();
}