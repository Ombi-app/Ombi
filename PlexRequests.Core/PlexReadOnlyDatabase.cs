#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: PlexReadOnlyDatabase.cs
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
using System.Collections.Generic;
using System.IO;
using PlexRequests.Core.SettingModels;
using PlexRequests.Store;
using PlexRequests.Store.Models.Plex;

namespace PlexRequests.Core
{
    public class PlexReadOnlyDatabase : IPlexReadOnlyDatabase
    {
        public PlexReadOnlyDatabase(IPlexDatabase plexDatabase, ISettingsService<PlexSettings> plexSettings)
        {
            Plex = plexDatabase;

            var settings = plexSettings.GetSettings();

            if (!string.IsNullOrEmpty(settings.PlexDatabaseLocationOverride))
            {
                //Overriden setting
                Plex.DbLocation = Path.Combine(settings.PlexDatabaseLocationOverride, "Plug-in Support", "Databases", "com.plexapp.plugins.library.db");
            }
            else if (Type.GetType("Mono.Runtime") != null)
            {
                // Mono
                Plex.DbLocation = Path.Combine("/var/lib/plexmediaserver/Library/Application Support/", "Plex Media Server", "Plug-in Support", "Databases", "com.plexapp.plugins.library.db");
            }
            else
            {
                // Default Windows
                Plex.DbLocation = Path.Combine(Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%"), "Plex Media Server", "Plug-in Support", "Databases", "com.plexapp.plugins.library.db");
            }
        }
        private IPlexDatabase Plex { get; }

        public IEnumerable<MetadataItems> GetItemsAddedAfterDate(DateTime dateTime)
        {
            // type 1 = Movie, type 4 = TV Episode
            return Plex.QueryMetadataItems(@"SELECT * FROM metadata_items 
                                            WHERE added_at > @AddedAt 
                                            AND metadata_type in (1,4)
                                            AND title <> ''",
                new { AddedAt = dateTime });
        }
    }
}