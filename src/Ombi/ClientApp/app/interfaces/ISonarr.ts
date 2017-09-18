import { ICutoff, IItem } from "./ICommon";

export interface ISonarrRootFolder {
    id: number;
    path: string;
    freespace: number;
}

export interface ISonarrProfile {
    name: string;
    id: number;
    cutoff: ICutoff;
    items: IItem[];
}
