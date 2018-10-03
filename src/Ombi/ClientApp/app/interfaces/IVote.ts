export interface IVoteViewModel {
    requestId: number;
    requestType: RequestTypes;
    image: string;
    background: string;
    upvotes: number;
    downvotes: number;
    title: string;
    description: string;
}

export interface IVoteEngineResult {
    result: boolean;
    message: string;
    isError: boolean;
    errorMessage: string;
}

export enum RequestTypes {
    TvShow = 0,
    Movie = 1,
    Album = 2,
}
