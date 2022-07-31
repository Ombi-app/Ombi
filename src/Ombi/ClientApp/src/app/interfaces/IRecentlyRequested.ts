import { RequestType } from "./IRequestModel";

export interface IRecentlyRequested {
    requestId: number;
    requestType: RequestType;
    userId: string;
    userName: string;
    available: boolean;
    tvPartiallyAvailable: boolean;
    requestDate: Date;
    title: string;
    overview: string;
    releaseDate: Date;
    approved: boolean;
    mediaId: string;

    posterPath: string;
}