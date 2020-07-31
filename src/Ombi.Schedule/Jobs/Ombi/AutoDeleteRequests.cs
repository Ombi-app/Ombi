using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core;
using Ombi.Core.Senders;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;
using Quartz;

namespace Ombi.Schedule.Jobs.Ombi
{
    public interface IAutoDeleteRequests : IBaseJob { }
    public class AutoDeleteRequests : IAutoDeleteRequests
    {

        private readonly ISettingsService<OmbiSettings> _ombiSettings;
        private readonly IMovieRequestRepository _movieRequests;

        public AutoDeleteRequests(ISettingsService<OmbiSettings> ombiSettings, IMovieRequestRepository movieRequest)
        {
            _ombiSettings = ombiSettings;
            _movieRequests = movieRequest;
        }

        public async Task Execute(IJobExecutionContext job)
        {
            var settings = await _ombiSettings.GetSettingsAsync();
            if (!settings.AutoDeleteAvailableRequests)
            {
                return;
            }
            await ProcessMovieRequests(settings.AutoDeleteAfterDays);
        }

        private async Task ProcessMovieRequests(int deleteAfterDays)
        {
            var date = DateTime.UtcNow.AddDays(-deleteAfterDays).Date;
            var requestsToDelete = await _movieRequests.GetAll().Where(x => x.Available && x.MarkedAsAvailable.HasValue && x.MarkedAsAvailable.Value < date).ToListAsync();

            foreach (var request in requestsToDelete)
            {

            }
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}