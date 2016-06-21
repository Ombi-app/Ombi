#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: StoreCleanup.cs
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
using System.Linq;

using NLog;

using PlexRequests.Services.Interfaces;
using PlexRequests.Store.Models;
using PlexRequests.Store.Repository;

using Quartz;

namespace PlexRequests.Services.Jobs
{
    public class StoreCleanup : IJob
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public StoreCleanup(IRepository<LogEntity> repo, IJobRecord rec)
        {
            Repo = repo;
            JobRecord = rec;
        }

        private IJobRecord JobRecord { get; }

        private IRepository<LogEntity> Repo { get; }

        private const int ItemsToDelete = 1000;

        private void Cleanup()
        {
            try
            {
                var items = Repo.GetAll();
                var ordered = items.OrderByDescending(x => x.Date).ToList();
                var itemsToDelete = new List<LogEntity>();
                if (ordered.Count > ItemsToDelete)
                {
                    itemsToDelete = ordered.Skip(ItemsToDelete).ToList();
                }

                foreach (var o in itemsToDelete)
                {
                    Repo.Delete(o);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            finally
            {
                JobRecord.Record(JobNames.StoreCleanup);
            }

        }

        public void Execute(IJobExecutionContext context)
        {
            Cleanup();
        }
    }
}