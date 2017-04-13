export interface ISettings {
    id:number
}

export interface IExternalSettings extends ISettings {
    ssl: boolean,
    subDir: boolean,
    ip: string,
    port:number
}

export interface IOmbiSettings extends ISettings {
    port: number,
//baseUrl:string,
    collectAnalyticData: boolean,
    wizard: boolean,
    apiKey:string
}

export interface IEmbySettings extends IExternalSettings {
    enable: boolean,
    apiKey: string,
    administratorId: string,
    enableEpisodeSearching:boolean
}

export interface IPlexSettings extends IExternalSettings {
    enable: boolean,
    enableEpisodeSearching: boolean,
    plexAuthToken: string,
    machineIdentifier: string
}