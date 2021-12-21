﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Core.Models;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.UI;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Engine
{
    public interface IMusicRequestEngine
    {
        Task<RequestEngineResult>ApproveAlbum(MusicRequests request);
        Task<RequestEngineResult> ApproveAlbumById(int requestId);
        Task<RequestEngineResult> DenyAlbumById(int modelId, string reason);
        Task<IEnumerable<MusicRequests>> GetRequests();
        Task<RequestsViewModel<MusicRequests>> GetRequests(int count, int position, OrderFilterModel orderFilter);
        Task<int> GetTotal();
        Task<RequestEngineResult> MarkAvailable(int modelId);
        Task<RequestEngineResult> MarkUnavailable(int modelId);
        Task<RequestEngineResult> RemoveAlbumRequest(int requestId);
        Task<RequestEngineResult> RequestAlbum(MusicAlbumRequestViewModel model);
        Task<RequestEngineResult> RequestArtist(MusicArtistRequestViewModel model);
        Task<IEnumerable<MusicRequests>> SearchAlbumRequest(string search);
        Task<bool> UserHasRequest(string userId);
        Task<RequestsViewModel<AlbumRequest>> GetRequestsByStatus(int count, int position, string sort, string sortOrder, RequestStatus available);
        Task<RequestsViewModel<AlbumRequest>> GetRequests(int count, int position, string sort, string sortOrder);

        Task<RequestQuotaCountModel> GetRemainingRequests(OmbiUser user = null);
        Task<RequestsViewModel<MusicRequests>> GetRequestsByStatus(int count, int position, string sort, string sortOrder, RequestStatus available);
        Task<RequestsViewModel<MusicRequests>> GetRequests(int count, int position, string sort, string sortOrder);
    }
}