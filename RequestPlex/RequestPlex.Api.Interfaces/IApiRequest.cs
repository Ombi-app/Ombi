using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RestSharp;

namespace RequestPlex.Api.Interfaces
{
    public interface IApiRequest
    {
        T Execute<T>(IRestRequest request, Uri baseUri) where T : new();
    }
}
