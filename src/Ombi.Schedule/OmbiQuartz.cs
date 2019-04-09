using System.Collections.Generic;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace Ombi.Schedule
{
    public class OmbiQuartz
    {
        private IScheduler _scheduler;
        
        public static IScheduler Scheduler => Instance._scheduler;

        // Singleton
        private static OmbiQuartz _instance;

        /// <summary>
        /// Singleton
        /// </summary>
        public static OmbiQuartz Instance => _instance ?? (_instance = new OmbiQuartz());

        private OmbiQuartz()
        {
            Init();
        }

        private async void Init()
        {
            _scheduler = await new StdSchedulerFactory().GetScheduler();
        }
        
        public IScheduler UseJobFactory(IJobFactory jobFactory)
        {
            Scheduler.JobFactory = jobFactory;
            return Scheduler;
        }

        public async void AddJob<T>(string name, string group, string cronExpression, Dictionary<string, string> jobData = null)
            where T : IJob
        {
            var jobBuilder = JobBuilder.Create<T>()
                .WithIdentity(name, group);
            if (jobData != null)
            {
                foreach (var o in jobData)
                {
                    jobBuilder.UsingJobData(o.Key, o.Value);
                }
            }

            var job = jobBuilder.Build();              
            
            ITrigger jobTrigger = TriggerBuilder.Create()
                .WithIdentity(name + "Trigger", group)

                .StartNow()

                .WithCronSchedule(cronExpression)
                .Build();
            
            await Scheduler.ScheduleJob(job, jobTrigger);
        }
        
        public static async void Start()
        {
            await Scheduler.Start();
        }
    }
}