using Microsoft.EntityFrameworkCore;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ombi.Core.Engine.V2
{

    public interface IIssuesEngine
    {
        Task<IEnumerable<IssuesSummaryModel>> GetIssues(int position, int take, IssueStatus status, CancellationToken token);
        Task<IssuesSummaryModel> GetIssuesByProviderId(string providerId, CancellationToken token);
    }

    public class IssuesEngine : IIssuesEngine
    {
        private readonly IRepository<IssueCategory> _categories;
        private readonly IRepository<Issues> _issues;
        private readonly IRepository<IssueComments> _comments;

        public IssuesEngine(IRepository<IssueCategory> categories,
            IRepository<Issues> issues,
            IRepository<IssueComments> comments)
        {
            _categories = categories;
            _issues = issues;
            _comments = comments;
        }

        public async Task<IEnumerable<IssuesSummaryModel>> GetIssues(int position, int take, IssueStatus status, CancellationToken token)
        {
            var issues = await _issues.GetAll().Where(x => x.Status == status && x.ProviderId != null).Skip(position).Take(take).OrderBy(x => x.Title).ToListAsync(token);
            var grouped = issues.GroupBy(x => x.Title, (key, g) => new { Title = key, Issues = g });
            
            var model = new List<IssuesSummaryModel>();

            foreach(var group in grouped)
            {
                model.Add(new IssuesSummaryModel
                {
                    Count = group.Issues.Count(),
                    Title = group.Title,
                    ProviderId = group.Issues.FirstOrDefault()?.ProviderId
                });
            }

            return model;
        }

        public async Task<IssuesSummaryModel> GetIssuesByProviderId(string providerId, CancellationToken token)
        {
            var issues = await _issues.GetAll().Include(x => x.Comments).ThenInclude(x => x.User).Include(x => x.UserReported).Include(x => x.IssueCategory).Where(x => x.ProviderId == providerId).ToListAsync(token);
            var grouped = issues.GroupBy(x => x.Title, (key, g) => new { Title = key, Issues = g }).FirstOrDefault();

            if (grouped == null)
            {
                return null;
            }

            return new IssuesSummaryModel
            {
                Count = grouped.Issues.Count(),
                Title = grouped.Title,
                ProviderId = grouped.Issues.FirstOrDefault()?.ProviderId,
                Issues = grouped.Issues
            };
        }

    }

    public class IssuesSummaryModel
    {
        public string Title { get; set; }
        public int Count { get; set; }
        public string ProviderId { get; set; }
        public IEnumerable<Issues> Issues { get; set; }
    }
}
