import { RequestType } from "./IRequestModel";

export interface IRecentlyRequested {
    requestId: number;
    userId: string;
    username: string;
    available: boolean;
    tvPartiallyAvailable: boolean;
    requestDate: Date;
    title: string;
    overview: string;
    releaseDate: Date;
    approved: boolean;
    mediaId: string;
    type: RequestType;

    posterPath: string;
    background: string;
}