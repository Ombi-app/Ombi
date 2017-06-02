using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;

namespace Ombi.Core.Engine.Interfaces
{
    public interface ITvRequestEngine : IRequestEngine<TvRequestModel>
    {

        Task RemoveTvRequest(int requestId);

        Task<RequestEngineResult> RequestTvShow(SearchTvShowViewModel tv);

        Task<IEnumerable<TvRequestModel>> SearchTvRequest(string search);

        Task<TvRequestModel> UpdateTvRequest(TvRequestModel request);
    }
}