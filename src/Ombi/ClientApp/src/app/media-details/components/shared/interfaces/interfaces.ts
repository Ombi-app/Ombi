import { RequestType } from "../../../../interfaces";

export interface IDenyDialogData {
    requestType: RequestType;
    requestId: number;
    denied: boolean;
}