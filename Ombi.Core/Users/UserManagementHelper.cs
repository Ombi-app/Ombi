using Ombi.Core.SettingModels;
using Ombi.Helpers.Permissions;

namespace Ombi.Core.Users
{
    public static class UserManagementHelper
    {

        public static int GetPermissions(UserManagementSettings settings)
        {
            var permission = 0;

            if (settings.AutoApproveMovies)
            {
                permission += (int)Permissions.AutoApproveMovie;
            }
            if (settings.AutoApproveMusic)
            {
                permission += (int)Permissions.AutoApproveAlbum;
            }
            if (settings.AutoApproveTvShows)
            {
                permission += (int)Permissions.AutoApproveTv;
            }
            if (settings.RequestMovies)
            {
                permission += (int)Permissions.RequestMovie;
            }
            if (settings.RequestMusic)
            {
                permission += (int)Permissions.RequestMusic;
            }
            if (settings.RequestTvShows)
            {
                permission += (int)Permissions.RequestTvShow;
            }
            if (settings.ReportIssues)
            {
                permission += (int)Permissions.ReportIssue;
            }
            if (settings.UsersCanViewOnlyOwnRequests)
            {
                permission += (int)Permissions.UsersCanViewOnlyOwnRequests;
            }
            if (settings.UsersCanViewOnlyOwnIssues)
            {
                permission += (int)Permissions.UsersCanViewOnlyOwnIssues;
            }


            return permission;
        }

        public static int GetFeatures(UserManagementSettings settings)
        {
            var features = 0;

            if (settings.RecentlyAddedNewsletter)
            {
                features += (int)Features.Newsletter;
            }
            if (settings.RecentlyAddedNotification)
            {
                features += (int)Features.RequestAddedNotification;
            }

            return features;
        }
    }
}