using System.Threading.Tasks;

namespace Ombi.Api
{
    public interface IApi
    {
        Task Request(Request request);
        Task<T> Request<T>(Request request);
        Task<string> RequestContent(Request request);
        T DeserializeXml<T>(string receivedString);
    }
}