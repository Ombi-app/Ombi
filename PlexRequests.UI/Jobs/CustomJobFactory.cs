#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: CustomJobFactory.cs
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

using NLog;

using PlexRequests.UI.Helpers;

using Quartz;
using Quartz.Spi;

namespace PlexRequests.UI.Jobs
{
    /// <summary>
    /// The custom job factory we are using so we are able to use our IoC container with DI in our Jobs.
    /// </summary>
    /// <seealso cref="Quartz.Spi.IJobFactory" />
    public class CustomJobFactory : IJobFactory
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private IServiceLocator Locator => ServiceLocator.Instance;

        /// <summary>
        /// Called by the scheduler at the time of the trigger firing, in order to
        /// produce a <see cref="T:Quartz.IJob" /> instance on which to call Execute.
        /// This will use the <see cref="IServiceLocator"/> to resolve all dependencies of a job.
        /// </summary>
        /// <param name="bundle">The TriggerFiredBundle from which the <see cref="T:Quartz.IJobDetail" />
        /// and other info relating to the trigger firing can be obtained.</param>
        /// <param name="scheduler">a handle to the scheduler that is about to execute the job</param>
        /// <returns>
        /// the newly instantiated Job
        /// </returns>
        /// <remarks>
        /// It should be extremely rare for this method to throw an exception -
        /// basically only the case where there is no way at all to instantiate
        /// and prepare the Job for execution.  When the exception is thrown, the
        /// Scheduler will move all triggers associated with the Job into the
        /// <see cref="F:Quartz.TriggerState.Error" /> state, which will require human
        /// intervention (e.g. an application restart after fixing whatever
        /// configuration problem led to the issue with instantiating the Job).
        /// </remarks>
        /// <throws>  SchedulerException if there is a problem instantiating the Job. </throws>
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            try
            {
                var jobDetail = bundle.JobDetail;
                var jobType = jobDetail.JobType;

                var resolvedType = (IJob)Locator.Resolve(jobType);
                return resolvedType;
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        /// <summary>
        /// Allows the job factory to destroy/clean-up the job if needed.
        /// </summary>
        /// <param name="job"></param>
        public void ReturnJob(IJob job)
        {
            // No need to do anything since our jobs do not implement IDisposable. 
        }
    }
}