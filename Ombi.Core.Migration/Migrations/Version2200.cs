﻿#region Copyright

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

using System.Data;
using NLog;
using Ombi.Core.SettingModels;
using Ombi.Store;

namespace Ombi.Core.Migration.Migrations
{
    [Migration(22000, "v2.20.0.0")]
    public class Version2200 : BaseMigration, IMigration
    {
        public Version2200(ISettingsService<CustomizationSettings> custom, ISettingsService<PlexSettings> ps)
        {
            Customization = custom;
            PlexSettings = ps;
        }

        public int Version => 22000;
        private ISettingsService<CustomizationSettings> Customization { get; set; }
        private ISettingsService<PlexSettings> PlexSettings { get; set; }


        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public void Start(IDbConnection con)
        {
            UpdatePlexSettings();
            UpdateCustomSettings();
            AddNewColumns(con);
            UpdateSchema(con, Version);
        }

        private void AddNewColumns(IDbConnection con)
        {
            con.AlterTable("EmbyContent", "ADD", "AddedAt", true, "VARCHAR(50)");
            con.AlterTable("EmbyEpisodes", "ADD", "AddedAt", true, "VARCHAR(50)");
        }

        private void UpdatePlexSettings()
        {
#if !DEBUG
            var s = PlexSettings.GetSettings();
            s.Enable = true;
            PlexSettings.SaveSettings(s);
#endif
        }
        private void UpdateCustomSettings()
        {

            var settings = Customization.GetSettings();
            settings.EnableIssues = true; 

            Customization.SaveSettings(settings);

        }
    }
}
