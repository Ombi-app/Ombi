﻿#region Copyright
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
    public class JellyfinContentRepository : MediaServerContentRepository<JellyfinContent>, IJellyfinContentRepository
    {

        public JellyfinContentRepository(ExternalContext db):base(db)
        {
        }

        public async Task<JellyfinContent> GetByImdbId(string imdbid)
        {
            return await Db.JellyfinContent.FirstOrDefaultAsync(x => x.ImdbId == imdbid);
        }
        public async Task<JellyfinContent> GetByTvDbId(string tv)
        {
            return await Db.JellyfinContent.FirstOrDefaultAsync(x => x.TvDbId == tv);
        }
        public async Task<JellyfinContent> GetByTheMovieDbId(string mov)
        {
            return await Db.JellyfinContent.FirstOrDefaultAsync(x => x.TheMovieDbId == mov);
        }

        public IQueryable<JellyfinContent> Get()
        {
            return Db.JellyfinContent.AsQueryable();
        }

        public async Task<JellyfinContent> GetByJellyfinId(string jellyfinId)
        {
            return await Db.JellyfinContent./*Include(x => x.Seasons).*/FirstOrDefaultAsync(x => x.JellyfinId == jellyfinId);
        }

        public override async Task Update(IMediaServerContent existingContent)
        {
            Db.JellyfinContent.Update((JellyfinContent)existingContent);
            await InternalSaveChanges();
        }

        public override IQueryable<IMediaServerEpisode> GetAllEpisodes()
        {
            return Db.JellyfinEpisode.AsQueryable();
        }

        public override async Task<IMediaServerEpisode> Add(IMediaServerEpisode content)
        {
            await Db.JellyfinEpisode.AddAsync((JellyfinEpisode)content);
            await InternalSaveChanges();
            return content;
        }
        public async Task<JellyfinEpisode> GetEpisodeByJellyfinId(string key)
        {
            return await Db.JellyfinEpisode.FirstOrDefaultAsync(x => x.JellyfinId == key);
        }

        public override async Task AddRange(IEnumerable<IMediaServerEpisode> content)
        {
            Db.JellyfinEpisode.AddRange((IEnumerable<JellyfinEpisode>)content);
            await InternalSaveChanges();
        }

        public override void UpdateWithoutSave(IMediaServerContent existingContent)
        {
            Db.JellyfinContent.Update((JellyfinContent)existingContent);
        }

        public override Task UpdateRange(IEnumerable<IMediaServerContent> existingContent)
        {
            Db.JellyfinContent.UpdateRange((IEnumerable<JellyfinContent>)existingContent);
            return InternalSaveChanges();
        }

        public override async Task DeleteTv(JellyfinContent tv)
        {
            var episodesToDelete = GetAllEpisodes().Cast<JellyfinEpisode>().Where(x => x.ParentId == tv.JellyfinId).ToList();
            Db.JellyfinEpisode.RemoveRange(episodesToDelete);
            await Delete(tv);
        }

        public override RecentlyAddedType RecentlyAddedType => RecentlyAddedType.Jellyfin;
    }
}
