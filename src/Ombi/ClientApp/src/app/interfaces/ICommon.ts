export interface ISettings {
    id: number;
}

export interface ICutoff {
    id: number;
    name: string;
}

export interface IItem {
    allowed: boolean;
    quality: IQuality;
}

export interface IQuality {
    id: number;
    name: string;
}

export interface ICheckbox {
    value: string;
    enabled: boolean;
}

export interface IUsersModel {
    id: string;
    username: string;
}

export interface INavBar {
    id: string;
    icon: string;
    name: string;
    link: string;
    requiresAdmin: boolean;
    enabled: boolean;
    toolTip?: boolean;
    toolTipMessage?: string;
    style?: string;
    externalLink?: boolean;
}