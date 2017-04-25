using System;

namespace Ombi.Notifications
{
    public enum NotificationType
    {
        NewRequest,
        Issue,
        RequestAvailable,
        RequestApproved,
        AdminNote,
        Test,
        RequestDeclined,
        ItemAddedToFaultQueue
    }
}
