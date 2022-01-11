#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: PlexContentRepository.cs
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
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Store.Context;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public class EmbyContentRepository : ExternalRepository<EmbyContent>, IEmbyContentRepository
    {

        public EmbyContentRepository(ExternalContext db):base(db)
        {
            Db = db;
        }

        private ExternalContext Db { get; }

        
        public async Task<EmbyContent> GetByImdbId(string imdbid)
        {
            return await Db.EmbyContent.FirstOrDefaultAsync(x => x.ImdbId == imdbid);
        }
        public async Task<EmbyContent> GetByTvDbId(string tv)
        {
            return await Db.EmbyContent.FirstOrDefaultAsync(x => x.TvDbId == tv);
        }
        public async Task<EmbyContent> GetByTheMovieDbId(string mov)
        {
            return await Db.EmbyContent.FirstOrDefaultAsync(x => x.TheMovieDbId == mov);
        }

        public IQueryable<EmbyContent> Get()
        {
            return Db.EmbyContent.AsQueryable();
        }

        public async Task<EmbyContent> GetByEmbyId(string embyId)
        {
            return await Db.EmbyContent./*Include(x => x.Seasons).*/FirstOrDefaultAsync(x => x.EmbyId == embyId);
        }

        public async Task Update(IMediaServerContent existingContent)
        {
            Db.EmbyContent.Update((EmbyContent)existingContent);
            await InternalSaveChanges();
        }

        public IQueryable<IMediaServerEpisode> GetAllEpisodes()
        {
            return Db.EmbyEpisode.AsQueryable();
        }

        public async Task<IMediaServerEpisode> Add(IMediaServerEpisode content)
        {
            await Db.EmbyEpisode.AddAsync((EmbyEpisode)content);
            await InternalSaveChanges();
            return content;
        }
        public async Task<EmbyEpisode> GetEpisodeByEmbyId(string key)
        {
            return await Db.EmbyEpisode.FirstOrDefaultAsync(x => x.EmbyId == key);
        }

        public async Task AddRange(IEnumerable<IMediaServerEpisode> content)
        {
            Db.EmbyEpisode.AddRange((EmbyEpisode)content);
            await InternalSaveChanges();
        }

        public void UpdateWithoutSave(IMediaServerContent existingContent)
        {
            Db.EmbyContent.Update((EmbyContent)existingContent);
        }
        
    }
}