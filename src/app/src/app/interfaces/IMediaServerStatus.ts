export interface IMediaServerStatus {
    serversAvailable: number;
    serversUnavailable: number;
    partiallyDown: boolean;
    completelyDown: boolean;
    fullyAvailable: boolean;
    totalServers: number;
}
