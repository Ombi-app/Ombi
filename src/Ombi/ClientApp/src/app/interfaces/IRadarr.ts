import { IChildRequests, IMovieRequests } from ".";
import { ITvRequests } from "./IRequestModel";

export interface IRadarrRootFolder {
    id: number;
    path: string;
}

export interface IRadarrProfile {
    name: string;
    id: number;
}

export interface IProfiles {
    name: string;
    id: number;
}

export interface IMinimumAvailability {
    value: string;
    name: string;
}
export interface IAdvancedData {
    profile: IRadarrProfile;
    profiles: IRadarrProfile[];
    profileId: number;
    rootFolder: IRadarrRootFolder;
    rootFolders: IRadarrRootFolder[];
    rootFolderId: number;
    movieRequest: IMovieRequests;
    tvRequest: ITvRequests;
}