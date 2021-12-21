using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ombi.Core.Engine
{
    public class RequestEngineResult
    {
        public bool Result { get; set; }
        public string Message { get; set; }
        public bool IsError => ( !string.IsNullOrEmpty(ErrorMessage) || ErrorCode != null );
        public string ErrorMessage { get; set; }
        public ErrorCode? ErrorCode { get; set; }
        public int RequestId { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ErrorCode {
        AlreadyRequested,
        EpisodesAlreadyRequested,
        NoPermissionsOnBehalf,
        NoPermissions,
        RequestDoesNotExist,
        ChildRequestDoesNotExist,
        NoPermissionsRequestMovie,
        NoPermissionsRequestTV,
        NoPermissionsRequestAlbum,
        MovieRequestQuotaExceeded,
        TvRequestQuotaExceeded,
        AlbumRequestQuotaExceeded,
    }
}