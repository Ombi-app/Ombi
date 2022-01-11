using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ombi.Helpers;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public interface IPlexContentRepository : IMediaServerContentRepository<PlexServerContent, PlexEpisode>
    {
        Task<bool> ContentExists(string providerId);
        Task<PlexServerContent> Get(string providerId, ProviderType type);
        Task<PlexServerContent> GetByType(string providerId, ProviderType type, MediaType MediaServerType);
        Task<PlexServerContent> GetByKey(int key);
        Task<PlexEpisode> GetEpisodeByKey(int key);
        IEnumerable<PlexServerContent> GetWhereContentByCustom(Expression<Func<PlexServerContent, bool>> predicate);
        Task<PlexServerContent> GetFirstContentByCustom(Expression<Func<PlexServerContent, bool>> predicate);
        Task DeleteEpisode(PlexEpisode content);
        void DeleteWithoutSave(PlexServerContent content);
        void DeleteWithoutSave(PlexEpisode content);
        Task UpdateRange(IEnumerable<PlexServerContent> existingContent);
    }
}