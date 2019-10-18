using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public interface IPlexContentRepository : IExternalRepository<PlexServerContent>
    {
        Task<bool> ContentExists(string providerId);
        Task<PlexServerContent> Get(string providerId);
        Task<PlexServerContent> GetByKey(int key);
        Task Update(PlexServerContent existingContent);
        IQueryable<PlexEpisode> GetAllEpisodes();
        Task<PlexEpisode> Add(PlexEpisode content);
        Task<PlexEpisode> GetEpisodeByKey(int key);
        Task AddRange(IEnumerable<PlexEpisode> content);
        IEnumerable<PlexServerContent> GetWhereContentByCustom(Expression<Func<PlexServerContent, bool>> predicate);
        Task<PlexServerContent> GetFirstContentByCustom(Expression<Func<PlexServerContent, bool>> predicate);
        Task DeleteEpisode(PlexEpisode content);
        void DeleteWithoutSave(PlexServerContent content);
        void DeleteWithoutSave(PlexEpisode content);
        Task UpdateRange(IEnumerable<PlexServerContent> existingContent);
        void UpdateWithoutSave(PlexServerContent existingContent);
    }
}