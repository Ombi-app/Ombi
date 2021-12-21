export interface IRequestEngineResult {
    result: boolean;
    message: string;
    errorMessage: string;
    errorCode: ErrorCode;
    requestId: number | undefined;
}

export enum ErrorCode {
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