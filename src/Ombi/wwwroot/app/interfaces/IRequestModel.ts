export interface IMediaBase {
    imdbId: string,
    id: number,
    providerId: number,
    title: string,
    overview: string,
    posterPath: string,
    releaseDate: Date,
    status: string,
    requestedDate: Date,
    approved: boolean,
    type: RequestType,
    requested: boolean,
    available: boolean,
    otherMessage: string,
    adminNote: string,
    requestedUser: string[],
    issueId: number,
    denied: boolean,
    deniedReason: string,
    released: boolean
}

export interface IMovieRequestModel extends IMediaBase {

}

export interface ITvRequestModel extends IMediaBase {
    imdbId: string,
    tvDbId: string,
    childRequests: IChildTvRequest[]
    rootFolderSelected: number,
    firstAired: string,
}

export interface IRequestCountModel {
    pending: number,
    approved: number,
    available: number
}

export interface IChildTvRequest extends IMediaBase {
    requestAll: boolean,
    seasonRequests: ISeasonRequests[],
}

export interface ISeasonRequests {
    seasonNumber: number,
    episodes: IEpisodesRequested[],
}

export interface IEpisodesRequested {
    episodeNumber: number,
    title: string,
    airDate: Date,
    url: string,
    requested: boolean,
    status: string,
    available: boolean
}


export enum RequestType {
    movie = 1,
    tvShow = 2
}

export interface IRequestsPageScroll {
    count: number,
    position: number
}