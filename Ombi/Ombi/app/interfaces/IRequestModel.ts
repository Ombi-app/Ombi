export interface IRequestModel {
    id: number,
    providerId: number,
    imdbId: string,
    overview: string,
    title: string,
    posterPath: string,
    releaseDate: Date,
    released: boolean,
    type: RequestType,
    status: string,
    approved: boolean,
    requestedUsers: string[],
    requestedDate: Date,
    releaseYear: string,
    available: boolean,
    issueId: number,
    denied: boolean,
    deniedReason: string,


}

export enum RequestType {
    movie = 1,
    tvShow = 2
}