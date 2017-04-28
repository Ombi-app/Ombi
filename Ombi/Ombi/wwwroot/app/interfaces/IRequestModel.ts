import { IEpisodeModel }from "./ISearchTvResult";

export interface IMediaBase {
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
    released:boolean
}

export interface IMovieRequestModel extends IMediaBase {
    imdbId: string,
}

export interface ITvRequestModel extends IMediaBase {
    imdbId: string,
    tvDbId: string,
    requestAll: boolean,
    seasonNumbersRequested: number[],
    episodes: IEpisodeModel[],
    childRequests: ITvRequestModel[],
    hasChildRequests: boolean,
    rootFolderSelected: number,
    firstAired:string,
}

export enum RequestType {
    movie = 1,
    tvShow = 2
}

export interface IRequestsPageScroll {
    count: number,
    position:number
}