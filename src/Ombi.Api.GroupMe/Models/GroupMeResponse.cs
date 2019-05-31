using System;
using Newtonsoft.Json;

namespace Ombi.Api.GroupMe.Models
{
    public class GroupMeResponse<T>
    {
        [JsonProperty(PropertyName = "response")]
        public T Response { get; set; }
        [JsonProperty(PropertyName = "meta")]
        public Meta Meta { get; set; }
    }
    public class Meta
    {
        public bool Successful
        {
            get
            {
                switch (StatusCode)
                {
                    case GroupMeStatusCode.OK:
                    case GroupMeStatusCode.Created:
                    case GroupMeStatusCode.NoContent:
                    case GroupMeStatusCode.NotModified:
                        return true;
                    case GroupMeStatusCode.BadRequest:
                    case GroupMeStatusCode.Unauthorized:
                    case GroupMeStatusCode.Forbidden:
                    case GroupMeStatusCode.NotFound:
                    case GroupMeStatusCode.EnhanceYourCalm:
                    case GroupMeStatusCode.InternalServerError:
                    case GroupMeStatusCode.BadGateway:
                    case GroupMeStatusCode.ServiceUnavailable:
                        return false;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(GroupMeStatusCode));
                }
            }
        }

        public GroupMeStatusCode StatusCode => (GroupMeStatusCode) code;
        public int code { get; set; }
        public string[] errors { get; set; }
    }

    public enum GroupMeStatusCode
    {   OK = 200,
        Created = 201, 
        NoContent = 204,
        NotModified = 304,
        BadRequest = 400,
        Unauthorized = 401,
        Forbidden = 403,
        NotFound = 404,
        EnhanceYourCalm = 420,
        InternalServerError = 500,
        BadGateway = 502,
        ServiceUnavailable = 503
    }

}