#region Copyright

// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: Version1100.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/

#endregion

using System;
using System.Data;
using NLog;
using Ombi.Core.SettingModels;
using Ombi.Store;
using Ombi.Store.Models;
using Ombi.Store.Models.Plex;
using Ombi.Store.Repository;
using Quartz.Collection;

namespace Ombi.Core.Migration.Migrations
{
    [Migration(22000, "v2.20.0.0")]
    public class Version2200 : BaseMigration, IMigration
    {
        public Version2200(ISettingsService<CustomizationSettings> custom, ISettingsService<PlexSettings> ps, IRepository<RecentlyAddedLog> log,
            IRepository<PlexContent> content, IRepository<PlexEpisodes> plexEp)
        {
            Customization = custom;
            PlexSettings = ps;
            Log = log;
            PlexContent = content;
            PlexEpisodes = plexEp;
        }

        public int Version => 22000;
        private ISettingsService<CustomizationSettings> Customization { get; }
        private ISettingsService<PlexSettings> PlexSettings { get; }
        private IRepository<RecentlyAddedLog> Log { get; }
        private IRepository<PlexContent> PlexContent { get; }
        private IRepository<PlexEpisodes> PlexEpisodes { get; }

        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public void Start(IDbConnection con)
        {
            UpdatePlexSettings();
            UpdateCustomSettings();
            AddNewColumns(con);
            UpdateSchema(con, Version);
            UpdateRecentlyAdded(con);

        }

        private void UpdateRecentlyAdded(IDbConnection con)
        {
            var allContent = PlexContent.GetAll();

            var content = new HashSet<RecentlyAddedLog>();
            foreach (var plexContent in allContent)
            {
                content.Add(new RecentlyAddedLog
                {
                    AddedAt = DateTime.UtcNow,
                    ProviderId = plexContent.ProviderId
                });
            }

            Log.BatchInsert(content, "RecentlyAddedLog");

            var allEp = PlexEpisodes.GetAll();
            content.Clear();
            foreach (var ep in allEp)
            {
                content.Add(new RecentlyAddedLog
                {
                    AddedAt = DateTime.UtcNow,
                    ProviderId = ep.RatingKey
                });
            }

            Log.BatchInsert(content, "RecentlyAddedLog");
        }

        private void AddNewColumns(IDbConnection con)
        {
            con.AlterTable("EmbyContent", "ADD", "AddedAt", true, "VARCHAR(50)");
            con.AlterTable("EmbyEpisodes", "ADD", "AddedAt", true, "VARCHAR(50)");

            con.AlterTable("PlexContent", "ADD", "ItemId", true, "VARCHAR(100)");
            con.AlterTable("PlexContent", "ADD", "AddedAt", true, "VARCHAR(100)");
        }

        private void UpdatePlexSettings()
        {
#if !DEBUG
            var s = PlexSettings.GetSettings();
            if (!string.IsNullOrEmpty(s.Ip))
            {
                s.Enable = true;
                PlexSettings.SaveSettings(s);
            }
#endif
        }
        private void UpdateCustomSettings()
        {

            var settings = Customization.GetSettings();
            settings.EnableIssues = true;
            settings.EnableNetflixResults = true;
            Customization.SaveSettings(settings);
        }
    }
}
