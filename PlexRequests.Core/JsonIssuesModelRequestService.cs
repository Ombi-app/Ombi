#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: IssueJsonService.cs
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using PlexRequests.Core.Models;
using PlexRequests.Helpers;
using PlexRequests.Store.Models;
using PlexRequests.Store.Repository;

namespace PlexRequests.Core
{
    public class IssueJsonService : IIssueService
    {
        public IssueJsonService(IRepository<IssueBlobs> repo)
        {
            Repo = repo;
        }
        private IRepository<IssueBlobs> Repo { get; }
        public async Task<int> AddIssueAsync(IssuesModel model)
        {
            var entity = new IssueBlobs { Type = model.Type, Content = ByteConverterHelper.ReturnBytes(model), RequestId = model.RequestId };
            var id = await Repo.InsertAsync(entity);

            model.Id = id;

            entity = new IssueBlobs { Type = model.Type, Content = ByteConverterHelper.ReturnBytes(model), RequestId = model.RequestId, Id = id };
            var result = await Repo.UpdateAsync(entity).ConfigureAwait(false);

            return result ? id : -1;
        }

        public async Task<bool> UpdateIssueAsync(IssuesModel model)
        {
            var entity = new IssueBlobs { Type = model.Type, Content = ByteConverterHelper.ReturnBytes(model), RequestId = model.RequestId, Id = model.Id };
            return await Repo.UpdateAsync(entity).ConfigureAwait(false);
        }

        public async Task DeleteIssueAsync(int id)
        {
            var entity = await Repo.GetAsync(id);

            await Repo.DeleteAsync(entity).ConfigureAwait(false);
        }

        public async Task DeleteIssueAsync(IssuesModel model)
        {
            var entity = new IssueBlobs { Type = model.Type, Content = ByteConverterHelper.ReturnBytes(model), RequestId = model.RequestId, Id = model.Id };

            await Repo.DeleteAsync(entity).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the Issues, if there is no issue in the DB we return a <c>new <see cref="IssuesModel"/></c>.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public async Task<IssuesModel> GetAsync(int id)
        {
            var blob = await Repo.GetAsync(id);

            if (blob == null)
            {
                return new IssuesModel();
            }
            var model = ByteConverterHelper.ReturnObject<IssuesModel>(blob.Content);
            return model;
        }

        /// <summary>
        /// Gets all the Issues, if there is no issue in the DB we return a <c>new IEnumerable&lt;IssuesModel&gt;</c>.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<IssuesModel>> GetAllAsync()
        {
            var blobs = await Repo.GetAllAsync().ConfigureAwait(false);

            if (blobs == null)
            {
                return new List<IssuesModel>();
            }
            return blobs.Select(b => Encoding.UTF8.GetString(b.Content))
                .Select(JsonConvert.DeserializeObject<IssuesModel>)
                .ToList();
        }
    }
}