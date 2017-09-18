﻿export interface ISettings {
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
