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

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Store.Context;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public class PlexContentRepository : IPlexContentRepository
    {

        public PlexContentRepository(IOmbiContext db)
        {
            Db = db;
        }

        private IOmbiContext Db { get; }

        public async Task<IEnumerable<PlexContent>> GetAll()
        {
            return await Db.PlexContent.ToListAsync();
        }

        public async Task AddRange(IEnumerable<PlexContent> content)
        {
            Db.PlexContent.AddRange(content);
            await Db.SaveChangesAsync();
        }

        public async Task<bool> ContentExists(string providerId)
        {
            return await Db.PlexContent.AnyAsync(x => x.ProviderId == providerId);
        }

        public async Task<PlexContent> Add(PlexContent content)
        {
            await Db.PlexContent.AddAsync(content);
            await Db.SaveChangesAsync();
            return content;
        }

        public async Task<PlexContent> Get(string providerId)
        {
            return await Db.PlexContent.FirstOrDefaultAsync(x => x.ProviderId == providerId);
        }

        public async Task<PlexContent> GetByKey(string key)
        {
            return await Db.PlexContent.Include(x => x.Seasons).FirstOrDefaultAsync(x => x.Key == key);
        }

        public async Task Update(PlexContent existingContent)
        {
            Db.PlexContent.Update(existingContent);
            await Db.SaveChangesAsync();
        }
    }
}