export interface ISettings {
    id:number
}

export interface IOmbiSettings extends ISettings {
    port: number,
//baseUrl:string,
    collectAnalyticData: boolean,
    wizard: boolean,
    apiKey:string
}