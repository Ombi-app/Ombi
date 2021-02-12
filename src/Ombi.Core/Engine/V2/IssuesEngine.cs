using Microsoft.EntityFrameworkCore;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ombi.Core.Engine.V2
{

    public interface IIssuesEngine
    {
        Task<IEnumerable<IssuesSummaryModel>> GetIssues(int position, int take, IssueStatus status);
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

        public async Task<IEnumerable<IssuesSummaryModel>> GetIssues(int position, int take, IssueStatus status)
        {
            var issues = await _issues.GetAll().Where(x => x.Status == status).Skip(position).Take(take).ToListAsync();
            var grouped = issues.GroupBy(x => x.Title, (key, g) => new { Title = key, Issues = g });
            
            var model = new List<IssuesSummaryModel>();

            foreach(var group in grouped)
            {
                model.Add(new IssuesSummaryModel
                {
                    Count = group.Issues.Count(),
                    Title = group.Title,
                    Issues = group.Issues
                });
            }

            return model;
        }

    }

    public class IssuesSummaryModel
    {
        public string Title { get; set; }
        public int Count { get; set; }
        public IEnumerable<Issues> Issues { get; set; }
    }
}
