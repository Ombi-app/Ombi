import { ICutoff, IItem } from "./ICommon";

export interface IRadarrRootFolder {
    id: number;
    path: string;
    freespace: number;
}

export interface IRadarrProfile {
    name: string;
    id: number;
    cutoff: ICutoff;
    items: IItem[];
}

export interface IMinimumAvailability {
    value: string;
    name: string;
}
