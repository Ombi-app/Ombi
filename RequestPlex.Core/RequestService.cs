#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: RequestService.cs
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
using System.Linq;

using RequestPlex.Store;

namespace RequestPlex.Core
{
    public class RequestService
    {
        public RequestService(ISqliteConfiguration db, IRepository<RequestedModel> repo)
        {
            Db = db;
            Repo = repo;
        }
        private ISqliteConfiguration Db { get; set; }
        private IRepository<RequestedModel> Repo { get; set; }
        public void AddRequest(int tmdbid, RequestType type)
        {
            var model = new RequestedModel
            {
                Tmdbid = tmdbid,
                Type = type
            };

            Repo.Insert(model);
        }

        public bool CheckRequest(int tmdbid)
        {
            return Repo.GetAll().Any(x => x.Tmdbid == tmdbid);
        }

        public void DeleteRequest(int tmdbId)
        {
            var entity = Repo.GetAll().FirstOrDefault(x => x.Tmdbid == tmdbId);
            Repo.Delete(entity);
        }

    }
}