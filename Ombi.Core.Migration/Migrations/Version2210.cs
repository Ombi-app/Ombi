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
using Ombi.Store.Models.Emby;
using Ombi.Store.Models.Plex;
using Ombi.Store.Repository;
using Quartz.Collection;

namespace Ombi.Core.Migration.Migrations
{
    [Migration(22100, "v2.21.0.0")]
    public class Version2210 : BaseMigration, IMigration
    {
        public Version2210(IRepository<RecentlyAddedLog> log,
            IRepository<PlexContent> content, IRepository<PlexEpisodes> plexEp, IRepository<EmbyContent> embyContent, IRepository<EmbyEpisodes> embyEp)
        {
            Log = log;
            PlexContent = content;
            PlexEpisodes = plexEp;
            EmbyContent = embyContent;
            EmbyEpisodes = embyEp;
        }

        public int Version => 22000;
        private IRepository<RecentlyAddedLog> Log { get; }
        private IRepository<PlexContent> PlexContent { get; }
        private IRepository<PlexEpisodes> PlexEpisodes { get; }
        private IRepository<EmbyContent> EmbyContent { get; }
        private IRepository<EmbyEpisodes> EmbyEpisodes { get; }
        
        public void Start(IDbConnection con)
        {
            UpdateRecentlyAdded(con);
            UpdateSchema(con, Version);

        }

        private void UpdateRecentlyAdded(IDbConnection con)
        {

            //Delete the recently added table, lets start again
            Log.DeleteAll("RecentlyAddedLog");



            // Plex 
            var plexAllContent = PlexContent.GetAll();
            var content = new HashSet<RecentlyAddedLog>();
            foreach (var plexContent in plexAllContent)
            {
                if(plexContent.Type == PlexMediaType.Artist) continue;
                content.Add(new RecentlyAddedLog
                {
                    AddedAt = DateTime.UtcNow,
                    ProviderId = plexContent.ProviderId
                });
            }
            Log.BatchInsert(content, "RecentlyAddedLog");

            var plexEpisodeses = PlexEpisodes.GetAll();
            content.Clear();
            foreach (var ep in plexEpisodeses)
            {
                content.Add(new RecentlyAddedLog
                {
                    AddedAt = DateTime.UtcNow,
                    ProviderId = ep.RatingKey
                });
            }
            Log.BatchInsert(content, "RecentlyAddedLog");

            // Emby 
            content.Clear();
            var embyContent = EmbyContent.GetAll();
            foreach (var plexContent in embyContent)
            {
                content.Add(new RecentlyAddedLog
                {
                    AddedAt = DateTime.UtcNow,
                    ProviderId = plexContent.EmbyId
                });
            }
            Log.BatchInsert(content, "RecentlyAddedLog");

            var embyEpisodes = EmbyEpisodes.GetAll();
            content.Clear();
            foreach (var ep in embyEpisodes)
            {
                content.Add(new RecentlyAddedLog
                {
                    AddedAt = DateTime.UtcNow,
                    ProviderId = ep.EmbyId
                });
            }
            Log.BatchInsert(content, "RecentlyAddedLog");


        }

     
    }
}
