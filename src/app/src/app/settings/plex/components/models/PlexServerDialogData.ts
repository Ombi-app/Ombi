import { IPlexServer } from "../../../../interfaces";

export interface PlexServerDialogData {
    server: IPlexServer;
    deleted?: boolean;
    closed?: boolean;
}