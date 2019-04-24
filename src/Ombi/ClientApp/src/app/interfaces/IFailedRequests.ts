import { RequestType } from ".";
export interface IFailedRequestsViewModel {
    failedId: number;
    title: string;
    releaseYear: Date;
    requestId: number;
    type: RequestType;
    dts: Date;
    error: string;
    retryCount: number;
}
