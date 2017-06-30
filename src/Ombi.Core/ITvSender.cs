using System.Threading.Tasks;
using Ombi.Api.Sonarr.Models;
using Ombi.Core.Settings.Models.External;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core
{
    public interface ITvSender
    {
        Task<NewSeries> SendToSonarr(ChildRequests model, string qualityId = null);
    }
}