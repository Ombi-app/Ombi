using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Repository.Requests;
using Quartz;

namespace Ombi.Schedule.Jobs.Ombi
{
    public class AutoDeleteRequests : IAutoDeleteRequests
    {

        private readonly ISettingsService<OmbiSettings> _ombiSettings;
        private readonly IMovieRequestRepository _movieRequests;
        private readonly ITvRequestRepository _tvRequestRepository;
        private readonly ILogger<AutoDeleteRequests> _logger;

        public AutoDeleteRequests(ISettingsService<OmbiSettings> ombiSettings, IMovieRequestRepository movieRequest,
            ILogger<AutoDeleteRequests> logger, ITvRequestRepository tvRequestRepository)
        {
            _ombiSettings = ombiSettings;
            _movieRequests = movieRequest;
            _tvRequestRepository = tvRequestRepository;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext job)
        {
            var settings = await _ombiSettings.GetSettingsAsync();
            if (!settings.AutoDeleteAvailableRequests)
            {
                return;
            }
            var date = DateTime.UtcNow.AddDays(-settings.AutoDeleteAfterDays).Date;
            await ProcessMovieRequests(date);
            await ProcessTvRequests(date);
        }

        private async Task ProcessMovieRequests(DateTime date)
        {
            var requestsToDelete = await _movieRequests.GetAll().Where(x => x.Available && x.MarkedAsAvailable.HasValue && x.MarkedAsAvailable.Value < date).ToListAsync();

            _logger.LogInformation($"Deleting {requestsToDelete.Count} movie requests that have now been scheduled for deletion, All available requests before {date::MM/dd/yyyy} will be deleted");
            foreach (var r in requestsToDelete)
            {
                _logger.LogInformation($"Deleting movie title {r.Title} as it was approved on {r.MarkedAsApproved:MM/dd/yyyy hh:mm tt}");
            }

            await _movieRequests.DeleteRange(requestsToDelete);
        }

        private async Task ProcessTvRequests(DateTime date)
        {
            var requestsToDelete = await _tvRequestRepository.GetChild().Where(x => x.Available && x.MarkedAsAvailable.HasValue && x.MarkedAsAvailable.Value < date).ToListAsync();

            _logger.LogInformation($"Deleting {requestsToDelete.Count} episode requests that have now been scheduled for deletion, All available requests before {date::MM/dd/yyyy} will be deleted");

            await _tvRequestRepository.DeleteChildRange(requestsToDelete);

            // Check if we have parent requests without any child requests now
            var parentRequests = await _tvRequestRepository.Get().Where(x => !x.ChildRequests.Any()).ToListAsync();

            await _tvRequestRepository.DeleteRange(parentRequests);
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