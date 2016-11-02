#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: IRequestService.cs
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

using PlexRequests.Store;

namespace PlexRequests.Core
{
    public interface IRequestService
    {
        long AddRequest(RequestedModel model);
        Task<int> AddRequestAsync(RequestedModel model);
        RequestedModel CheckRequest(int providerId);
        Task<RequestedModel> CheckRequestAsync(int providerId);
        RequestedModel CheckRequest(string musicId);
        Task<RequestedModel> CheckRequestAsync(string musicId);
        
        void DeleteRequest(RequestedModel request);
        Task DeleteRequestAsync(RequestedModel request);
        bool UpdateRequest(RequestedModel model);
        Task<bool> UpdateRequestAsync(RequestedModel model);
        RequestedModel Get(int id);
        Task<RequestedModel> GetAsync(int id);
        IEnumerable<RequestedModel> GetAll();
        Task<IEnumerable<RequestedModel>> GetAllAsync();
        bool BatchUpdate(IEnumerable<RequestedModel> model);
        Task<bool> BatchUpdateAsync(IEnumerable<RequestedModel> model);
        bool BatchDelete(IEnumerable<RequestedModel> model);
        Task<bool> BatchDeleteAsync(IEnumerable<RequestedModel> model);
    }
}