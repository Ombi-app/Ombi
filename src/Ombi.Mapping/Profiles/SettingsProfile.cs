using AutoMapper;
using Ombi.Models.Notifications;
using Ombi.Settings.Settings.Models.Notifications;

namespace Ombi.Mapping.Profiles
{
    public class SettingsProfile : Profile
    {
        public SettingsProfile()
        {
            CreateMap<EmailNotificationsViewModel, EmailNotificationSettings>().ReverseMap();
        }
    }
}