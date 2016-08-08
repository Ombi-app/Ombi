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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NLog;

using PlexRequests.Api.Interfaces;
using PlexRequests.Api.Models.Plex;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.Services.Interfaces;
using PlexRequests.Services.Models;
using PlexRequests.Store.Models;
using PlexRequests.Store.Repository;

using Quartz;

namespace PlexRequests.Services.Jobs
{
    public class PlexEpisodeCacher : IJob
    {
        public PlexEpisodeCacher(ISettingsService<PlexSettings> plexSettings, IPlexApi plex, ICacheProvider cache,
            IJobRecord rec, IRepository<PlexEpisodes> repo, ISettingsService<ScheduledJobsSettings> jobs)
        {
            Plex = plexSettings;
            PlexApi = plex;
            Cache = cache;
            Job = rec;
            Repo = repo;
            Jobs = jobs;
        }

        private ISettingsService<PlexSettings> Plex { get; }
        private static Logger Log = LogManager.GetCurrentClassLogger();
        private IPlexApi PlexApi { get; }
        private ICacheProvider Cache { get; }
        private IJobRecord Job { get; }
        private IRepository<PlexEpisodes> Repo { get; }
        private ISettingsService<ScheduledJobsSettings> Jobs { get; }
        private const int ResultCount = 25;
        private const string PlexType = "episode";
        private const string TableName = "PlexEpisodes";


        public void CacheEpisodes()
        {
            var videoHashset = new HashSet<Video>();
            var settings = Plex.GetSettings();
            // Ensure Plex is setup correctly
            if (string.IsNullOrEmpty(settings.PlexAuthToken))
            {
                return;
            }

            // Get the librarys and then get the tv section
            var sections = PlexApi.GetLibrarySections(settings.PlexAuthToken, settings.FullUri);
            var tvSection = sections.Directories.FirstOrDefault(x => x.type.Equals(PlexMediaType.Show.ToString(), StringComparison.CurrentCultureIgnoreCase));
            var tvSectionId = tvSection?.Key;

            var currentPosition = 0;
            int totalSize;

            // Get the first 25 episodes (Paged)
            var episodes = PlexApi.GetAllEpisodes(settings.PlexAuthToken, settings.FullUri, tvSectionId, currentPosition, ResultCount);

            // Parse the total amount of episodes
            int.TryParse(episodes.TotalSize, out totalSize);

            // Get all of the episodes in batches until we them all (Got'a catch 'em all!)
            while (currentPosition < totalSize)
            {
                videoHashset.UnionWith(PlexApi.GetAllEpisodes(settings.PlexAuthToken, settings.FullUri, tvSectionId, currentPosition, ResultCount).Video
                    .Where(x => x.Type.Equals(PlexType, StringComparison.InvariantCultureIgnoreCase)));
                currentPosition += ResultCount;
            }

            var entities = new ConcurrentDictionary<PlexEpisodes, byte>();

            Parallel.ForEach(videoHashset, video =>
            {
                // Get the individual episode Metadata (This is for us to get the TheTVDBId which also includes the episode number and season number)
                var metadata = PlexApi.GetEpisodeMetaData(settings.PlexAuthToken, settings.FullUri, video.RatingKey);

                // Loop through the metadata and create the model to insert into the DB
                foreach (var metadataVideo in metadata.Video)
                {
                    var epInfo = PlexHelper.GetSeasonsAndEpisodesFromPlexGuid(metadataVideo.Guid);
                    entities.TryAdd(
                        new PlexEpisodes
                        {
                            EpisodeNumber = epInfo.EpisodeNumber,
                            EpisodeTitle = metadataVideo.Title,
                            ProviderId = epInfo.ProviderId,
                            RatingKey = metadataVideo.RatingKey,
                            SeasonNumber = epInfo.SeasonNumber,
                            ShowTitle = metadataVideo.GrandparentTitle
                        },
                        1);
                }
            });

            // Delete all of the current items
            Repo.DeleteAll(TableName);

            // Insert the new items
            var result = Repo.BatchInsert(entities.Select(x => x.Key).ToList(), TableName, typeof(PlexEpisodes).GetPropertyNames());

            if (!result)
            {
                Log.Error("Saving the Plex episodes to the DB Failed");
            }
        }

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                var jobs = Job.GetJobs();
                var job = jobs.FirstOrDefault(x => x.Name.Equals(JobNames.EpisodeCacher, StringComparison.CurrentCultureIgnoreCase));
                if (job != null)
                {
                    if (job.LastRun > DateTime.Now.AddHours(-1)) // If it's been run in the last hour
                    {
                        return;
                    }
                }
                CacheEpisodes();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            finally
            {
                Job.Record(JobNames.EpisodeCacher);
            }
        }
    }
}