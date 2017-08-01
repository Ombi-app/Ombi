export interface ISettings {
    id: number
}

export interface IExternalSettings extends ISettings {
    ssl: boolean,
    subDir: string,
    ip: string,
    port: number,
}

export interface IOmbiSettings extends ISettings {
    port: number,
    //baseUrl:string, 
    collectAnalyticData: boolean,
    wizard: boolean,
    apiKey: string,
    externalUrl: string,
    allowExternalUsersToAuthenticate:boolean,
}

export interface IEmbySettings extends IExternalSettings {
    apiKey: string,
    enable: boolean,
    administratorId: string,
    enableEpisodeSearching: boolean,
}

export interface IPlexSettings extends ISettings {

    enable: boolean,
    servers: IPlexServer[]
}

export interface IPlexServer extends IExternalSettings {
    name: string,
    enableEpisodeSearching: boolean,
    plexAuthToken: string,
    machineIdentifier: string,
    plexSelectedLibraries: IPlexLibraries[],
}

export interface IPlexLibraries {
    key: string,
    title: string,
    enabled: boolean,
}

export interface ISonarrSettings extends IExternalSettings {
    apiKey: string,
    enabled: boolean,
    qualityProfile: string,
    seasonFolders: boolean,
    rootPath: string,
    fullRootPath: string,
    addOnly:boolean,
}

export interface IRadarrSettings extends IExternalSettings {
    enabled: boolean,
    apiKey: string,
    defaultQualityProfile: string,
    defaultRootPath: string,
    fullRootPath: string,
    addOnly: boolean,
    minimumAvailability:string,
}

export interface ILandingPageSettings extends ISettings {
    enabled: boolean,
    beforeLogin: boolean,
    afterLogin: boolean,
    noticeEnabled: boolean,
    noticeText: string,
    noticeBackgroundColor: string,
    timeLimit: boolean,
    startDateTime: Date,
    endDateTime: Date,
}

export interface ICustomizationSettings extends ISettings {
    applicationName: string,
    logo: string,
}