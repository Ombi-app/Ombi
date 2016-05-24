#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: StoreBackup.cs
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
using System.IO;
using System.Linq;
using System.Globalization;

using NLog;

using PlexRequests.Services.Interfaces;
using PlexRequests.Store;
using PlexRequests.Store.Models;
using PlexRequests.Store.Repository;

using Quartz;


namespace PlexRequests.Services.Jobs
{
    public class StoreBackup : IJob
    {
        public StoreBackup(ISqliteConfiguration sql, IJobRecord rec)
        {
            Sql = sql;
            JobRecord = rec;
        }

        private ISqliteConfiguration Sql { get; }
        private IJobRecord JobRecord { get; }

        private static Logger Log = LogManager.GetCurrentClassLogger();

        public void Execute(IJobExecutionContext context)
        {
            TakeBackup();
			Cleanup ();
        }

        private void TakeBackup()
        {
            Log.Trace("Starting DB Backup");
            var dbPath = Sql.CurrentPath;
            var dir = Path.GetDirectoryName(dbPath);
            if (dir == null)
            {
                Log.Warn("We couldn't find the DB path. We cannot backup.");
                return;
            }
            var backupDir = Directory.CreateDirectory(Path.Combine(dir, "Backup"));


            if (string.IsNullOrEmpty(dbPath))
            {
                Log.Warn("Could not find the actual database. We cannot backup.");
                return;
            }

            try
            {
				if(DoWeNeedToBackup(backupDir.FullName))
				{
                File.Copy(dbPath, Path.Combine(backupDir.FullName, $"PlexRequests.sqlite_{DateTime.Now.ToString("yyyy-MM-dd hh.mm.ss")}.bak"));
				}
				}
            catch (Exception e)
            {
                Log.Warn(e);
                Log.Warn("Exception when trying to copy the backup.");
            }
            finally
            {
                JobRecord.Record(JobNames.StoreBackup);
            }

        }

		private void Cleanup()
		{
			Log.Trace("Starting DB Cleanup");
			var dbPath = Sql.CurrentPath;
			var dir = Path.GetDirectoryName(dbPath);
			if (dir == null)
			{
				Log.Warn("We couldn't find the DB path. We cannot backup.");
				return;
			}
			var backupDir = Directory.CreateDirectory(Path.Combine(dir, "Backup"));

			var files = backupDir.GetFiles();

			foreach (var file in files) {
				var dt = ParseName(file.Name);
				if(dt < DateTime.Now.AddDays(-7)){
					try {

						File.Delete(file.FullName);
					} catch (Exception ex) {
						Log.Error(ex);
					}
				}
			}

		}

		private bool DoWeNeedToBackup(string backupPath)
		{
			var files = Directory.GetFiles(backupPath);
			//TODO Get the latest file and if it's within an hour of DateTime.Now then don't bother backing up.
			return true;
		}

		private DateTime ParseName(string fileName)
		{
			var names = fileName.Split(new []{'_','.',' '}, StringSplitOptions.RemoveEmptyEntries);
			if(names.Count() > 1)
			{
				DateTime parsed;
				//DateTime.TryParseExcat(names[1], "yyyy-MM-dd hh.mm.ss",CultureInfo.CurrentUICulture, DateTimeStyles.None, out parsed);
				DateTime.TryParse(names[2], out parsed);
				return parsed;

			}
			return DateTime.MinValue;
		}
    }
}