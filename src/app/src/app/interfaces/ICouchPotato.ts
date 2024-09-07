export interface ICouchPotatoProfiles {
    success: boolean;
    list: IProfileList[];
}

export interface IProfileList {
    core: boolean;
    hide: boolean;
    _rev: string;
    finish: boolean[];
    qualities: string[];
    _id: string;
    _t: string;
    label: string;
    minimum_score: number;
    stop_after: number[];
    wait_for: object[];
    order: number;
    threeD: object[];
}

export interface ICouchPotatoApiKey {
    success: boolean;
    api_key: string;
}
