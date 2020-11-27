import { RequestType } from "../../../../interfaces";

export interface IDenyDialogData {
    requestType: RequestType;
    requestId: number;
    denied: boolean;
}

export interface IIssueDialogData {
    requestType: RequestType;
    requestId: number;
    providerId: string;
    title: string;
}