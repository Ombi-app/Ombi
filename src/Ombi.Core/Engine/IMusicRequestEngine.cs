﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.UI;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Engine
{
    public interface IMusicRequestEngine
    {
        Task<RequestEngineResult>ApproveAlbum(AlbumRequest request);
        Task<RequestEngineResult> ApproveAlbumById(int requestId);
        Task<RequestEngineResult> DenyAlbumById(int modelId);
        Task<IEnumerable<AlbumRequest>> GetRequests();
        Task<RequestsViewModel<AlbumRequest>> GetRequests(int count, int position, OrderFilterModel orderFilter);
        Task<int> GetTotal();
        Task<RequestEngineResult> MarkAvailable(int modelId);
        Task<RequestEngineResult> MarkUnavailable(int modelId);
        Task RemoveAlbumRequest(int requestId);
        Task<RequestEngineResult> RequestAlbum(MusicAlbumRequestViewModel model);
        Task<IEnumerable<AlbumRequest>> SearchAlbumRequest(string search);
        Task<bool> UserHasRequest(string userId);
    }
}