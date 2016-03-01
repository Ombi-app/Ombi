using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RequestPlex.Api.Interfaces;

using RestSharp;

namespace RequestPlex.Api
{
    public class ApiRequest : IApiRequest
    {
        
        /// <summary>
        /// An API request handler
        /// </summary>
        /// <typeparam name="T">The type of class you want to deserialize</typeparam>
        /// <param name="request">The request.</param>
        /// <param name="baseUri">The base URI.</param>
        /// <returns>The type of class you want to deserialize</returns>
        public T Execute<T>(IRestRequest request, Uri baseUri) where T : new()
        {
            var client = new RestClient { BaseUrl = baseUri };

            var response = client.Execute<T>(request);

            if (response.ErrorException != null)
            { 
                var message = "Error retrieving response. Check inner details for more info.";
                throw new ApplicationException(message, response.ErrorException);
            }

            return response.Data;

        }
    }
}
