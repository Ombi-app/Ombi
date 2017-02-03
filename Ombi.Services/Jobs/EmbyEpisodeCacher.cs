#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: PlexEpisodeCacher.cs
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
using Ombi.Api.Interfaces;
using Ombi.Api.Models.Emby;
using Ombi.Core;
using Ombi.Core.SettingModels;
using Ombi.Helpers;
using Ombi.Services.Interfaces;
using Ombi.Services.Jobs.Interfaces;
using Ombi.Store.Models.Emby;
using Ombi.Store.Repository;
using Quartz;

namespace Ombi.Services.Jobs
{
    public class EmbyEpisodeCacher : IJob, IEmbyEpisodeCacher
    {
        public EmbyEpisodeCacher(ISettingsService<EmbySettings> embySettings, IEmbyApi emby, ICacheProvider cache,
            IJobRecord rec, IRepository<EmbyEpisodes> repo, ISettingsService<ScheduledJobsSettings> jobs)
        {
            Emby = embySettings;
            EmbyApi = emby;
            Cache = cache;
            Job = rec;
            Repo = repo;
            Jobs = jobs;
        }

        private ISettingsService<EmbySettings> Emby { get; }
        private static Logger Log = LogManager.GetCurrentClassLogger();
        private IEmbyApi EmbyApi { get; }
        private ICacheProvider Cache { get; }
        private IJobRecord Job { get; }
        private IRepository<EmbyEpisodes> Repo { get; }
        private ISettingsService<ScheduledJobsSettings> Jobs { get; }

        private const string TableName = "EmbyEpisodes";


        public void CacheEpisodes(EmbySettings settings)
        {
            var allEpisodes = EmbyApi.GetAllEpisodes(settings.ApiKey, settings.AdministratorId, settings.FullUri);
            var model = new List<EmbyEpisodes>();
            foreach (var ep in allEpisodes.Items)
            {
                var epInfo = EmbyApi.GetInformation(ep.Id, EmbyMediaType.Episode, settings.ApiKey,
                    settings.AdministratorId, settings.FullUri);
				if (epInfo.EpisodeInformation?.ProviderIds?.Tvdb == null)
				{
					continue;
				}
                model.Add(new EmbyEpisodes
                {
                    EmbyId = ep.Id,
                    EpisodeNumber = ep.IndexNumber,
                    SeasonNumber = ep.ParentIndexNumber,
                    EpisodeTitle = ep.Name,
                    ParentId = ep.SeriesId,
                    ShowTitle = ep.SeriesName,
                    ProviderId = epInfo.EpisodeInformation.ProviderIds.Tvdb
                });
            }

            // Delete all of the current items
            Repo.DeleteAll(TableName);

            // Insert the new items
            var result = Repo.BatchInsert(model, TableName, typeof(EmbyEpisodes).GetPropertyNames());

            if (!result)
            {
                Log.Error("Saving the emby episodes to the DB Failed");
            }
        }

        public void Start()
        {
            try
            {
                var s = Emby.GetSettings();
                if (!s.EnableEpisodeSearching)
                {
                    return;
                }

                Job.SetRunning(true, JobNames.EmbyEpisodeCacher);
                CacheEpisodes(s);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            finally
            {
                Job.Record(JobNames.EmbyEpisodeCacher);
                Job.SetRunning(false, JobNames.EmbyEpisodeCacher);
            }
        }
        public void Execute(IJobExecutionContext context)
        {

            try
            {
                var s = Emby.GetSettings();
                if (!s.EnableEpisodeSearching)
                {
                    return;
                }

                Job.SetRunning(true, JobNames.EmbyEpisodeCacher);
                CacheEpisodes(s);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            finally
            {
                Job.Record(JobNames.EmbyEpisodeCacher);
                Job.SetRunning(false, JobNames.EmbyEpisodeCacher);
            }
        }
    }
}