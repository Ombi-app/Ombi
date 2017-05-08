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
    released:boolean
}

export interface IMovieRequestModel extends IMediaBase {

}

export interface ITvRequestModel extends IMediaBase {
    imdbId: string,
    tvDbId: string,
    requestAll: boolean,
    seasonRequests: ISeasonRequests[],
    childRequests: ITvRequestModel[],
    hasChildRequests: boolean,
    rootFolderSelected: number,
    firstAired:string,
}

export interface ISeasonRequests
{
    seasonNumber: number,
    episodes:number[],
}


export enum RequestType {
    movie = 1,
    tvShow = 2
}

export interface IRequestsPageScroll {
    count: number,
    position:number
}