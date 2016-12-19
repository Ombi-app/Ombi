#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: JobRecord.cs
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
using System.Threading.Tasks;
using Ombi.Services.Interfaces;
using Ombi.Store.Models;
using Ombi.Store.Repository;

namespace Ombi.Services.Jobs
{
    public class JobRecord : IJobRecord
    {
        public JobRecord(IRepository<ScheduledJobs> repo)
        {
            Repo = repo;
        }

        private IRepository<ScheduledJobs> Repo { get; }

        public void Record(string jobName)
        {
            var allJobs = Repo.GetAll();
            var storeJob = allJobs.FirstOrDefault(x => x.Name == jobName);
            if (storeJob != null)
            {
                storeJob.LastRun = DateTime.UtcNow;
                Repo.Update(storeJob);
            }
            else
            {
                var job = new ScheduledJobs { LastRun = DateTime.UtcNow, Name = jobName };
                Repo.Insert(job);
            }
        }

        public void SetRunning(bool running, string jobName)
        {
            var allJobs = Repo.GetAll();
            var storeJob = allJobs.FirstOrDefault(x => x.Name == jobName);
            if (storeJob != null)
            {
                storeJob.Running = running;
                Repo.Update(storeJob);
            }
            else
            {
                var job = new ScheduledJobs { Running = running, Name = jobName };
                Repo.Insert(job);
            }
        }


        public async Task<IEnumerable<ScheduledJobs>> GetJobsAsync()
        {
            return await Repo.GetAllAsync();
        }

        public IEnumerable<ScheduledJobs> GetJobs()
        {
            return Repo.GetAll();
        }
    }
}