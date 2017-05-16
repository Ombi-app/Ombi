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
using NLog;
using Ombi.Services.Interfaces;
using Ombi.Store;
using Quartz;

namespace Ombi.Services.Jobs
{
    public class StoreBackup : IJob, IStoreBackup
    {
        public StoreBackup(ISqliteConfiguration sql, IJobRecord rec)
        {
            Sql = sql;
            JobRecord = rec;
        }

        private ISqliteConfiguration Sql { get; }
        private IJobRecord JobRecord { get; }

        private static Logger Log = LogManager.GetCurrentClassLogger();

        public void Start()
        {
            JobRecord.SetRunning(true, JobNames.CpCacher);
            TakeBackup();
            Cleanup();
        }

        public void Execute(IJobExecutionContext context)
        {
            JobRecord.SetRunning(true, JobNames.CpCacher);
            TakeBackup();
            Cleanup();
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
                if (DoWeNeedToBackup(backupDir.FullName))
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
                JobRecord.SetRunning(false, JobNames.CpCacher);
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

            foreach (var file in files)
            {
                var dt = ParseName(file.Name);
                if (dt < DateTime.Now.AddDays(-7))
                {
                    try
                    {

                        File.Delete(file.FullName);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                }
            }

        }

        private bool DoWeNeedToBackup(string backupPath)
        {
            var files = Directory.GetFiles(backupPath);
            var last = files.LastOrDefault();
            if (!string.IsNullOrEmpty(last))
            {
                var dt = ParseName(Path.GetFileName(last));
                if (dt < DateTime.Now.AddHours(-1))
                {
                    return true;
                }
                return false;
            }

            // We don't have a backup
            return true;
        }

        private DateTime ParseName(string fileName)
        {
            var names = fileName.Split(new[] { '_', '.', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (names.Length > 1)
            {
                DateTime parsed;
                DateTime.TryParse(names[2], out parsed);
                return parsed;

            }
            return DateTime.MinValue;
        }
    }
}