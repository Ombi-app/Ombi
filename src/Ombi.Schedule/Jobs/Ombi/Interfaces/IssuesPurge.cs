using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Quartz;

namespace Ombi.Schedule.Jobs.Ombi
{
    public class IssuesPurge : IIssuesPurge
    {
        public IssuesPurge(IRepository<Issues> issuesRepo, ISettingsService<IssueSettings> issueSettings)
        {
            _issuesRepository = issuesRepo;
            _settings = issueSettings;
            _settings.ClearCache();
        }

        private readonly IRepository<Issues> _issuesRepository;
        private readonly ISettingsService<IssueSettings> _settings;

        public async Task Execute(IJobExecutionContext job)
        {
            var settings = await _settings.GetSettingsAsync();
            if (!settings.DeleteIssues)
            {
                return;
            }

            var today = DateTime.UtcNow.Date;
            
            var resolved = await _issuesRepository.GetAll().Where(x => x.Status == IssueStatus.Resolved).ToListAsync();
            var toDelete = resolved.Where(x => x.ResovledDate.HasValue && (today - x.ResovledDate.Value.Date).TotalDays >= settings.DaysAfterResolvedToDelete);

            foreach (var d in toDelete)
            {
                d.Status = IssueStatus.Deleted;
            }

            await _issuesRepository.SaveChangesAsync();
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                //_settings?.Dispose();
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