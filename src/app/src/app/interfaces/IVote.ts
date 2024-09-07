export interface IVoteViewModel {
    requestId: number;
    requestType: RequestTypes;
    image: string;
    background: string;
    upvotes: number;
    downvotes: number;
    title: string;
    description: string;
    alreadyVoted: boolean;
    myVote: VoteType;
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

export enum VoteType {
    Upvote = 0,
    Downvote = 1,
}
