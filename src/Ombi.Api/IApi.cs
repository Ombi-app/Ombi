using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Ombi.Api
{
    public interface IApi
    {
        Task<HttpResponseMessage> Request(Request request, CancellationToken token = default(CancellationToken));
        Task<T> Request<T>(Request request, CancellationToken cancellationToken = default(CancellationToken));
        Task<string> RequestContent(Request request);
        T DeserializeXml<T>(string receivedString);
    }
}