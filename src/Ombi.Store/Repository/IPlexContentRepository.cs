﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public interface IPlexContentRepository : IRepository<PlexServerContent>
    {
        Task<bool> ContentExists(string providerId);
        Task<PlexServerContent> Get(string providerId);
        Task<PlexServerContent> GetByKey(int key);
        Task Update(PlexServerContent existingContent);
        IQueryable<PlexEpisode> GetAllEpisodes();
        Task<PlexEpisode> Add(PlexEpisode content);
        Task<PlexEpisode> GetEpisodeByKey(int key);
        Task AddRange(IEnumerable<PlexEpisode> content);
    }
}