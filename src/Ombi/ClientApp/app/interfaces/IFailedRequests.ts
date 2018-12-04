import { RequestType } from ".";
export interface IFailedRequestsViewModel {
    failedId: number;
    title: string;
    releaseYear: Date;
    requestId: number;
    requestType: RequestType;
    dts: Date;
    error: string;
    retryCount: number;
}
