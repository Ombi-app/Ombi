using System.Collections.Generic;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace Ombi.Schedule
{
    public class OmbiQuartz
    {
        protected IScheduler _scheduler { get; set; }
        
        public static IScheduler Scheduler => Instance._scheduler;

        // Singleton
        protected static OmbiQuartz _instance;

        /// <summary>
        /// Singleton
        /// </summary>
        public static OmbiQuartz Instance => _instance ?? (_instance = new OmbiQuartz());

        protected OmbiQuartz()
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

        public async Task AddJob<T>(string name, string group, string cronExpression, Dictionary<string, string> jobData = null)
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
                .WithCronSchedule(cronExpression)
                .Build();
            
            await Scheduler.ScheduleJob(job, jobTrigger);
        }

        public static async Task TriggerJob(string jobName)
        {
            await Scheduler.TriggerJob(new JobKey(jobName));
        }
        
        public static async Task Start()
        {
            await Scheduler.Start();
        }
    }
}