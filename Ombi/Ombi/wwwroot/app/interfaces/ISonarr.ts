export interface ISonarrRootFolder {
    id: number,
    path: string,
    freespace:number,
}

export interface ISonarrProfile {
    name: string,
    id: number,
    cutoff: ICutoff,
    items:IItem[],
}

export interface ICutoff {
    id: number,
    name:string,
}

export interface IItem {
    allowed: boolean,
    quality:IQuality,
}

export interface IQuality {
    id: number,
    name:string,
}