import { INewSeasonRequests } from "./IRequestModel";

export interface ISearchTvResultV2 {
    id: number;
    title: string; // used in the request
    aliases: string[];
    banner: string;
    seriesId: number;
    status: string;
    firstAired: string;
    networkId: string;
    runtime: string;
    genre: string[];
    overview: string;
    lastUpdated: number;
    airsDayOfWeek: string;
    airsTime: string;
    rating: string;
    imdbId: string;
    siteRating: number;
    trailer: string;
    homepage: string;
    seasonRequests: INewSeasonRequests[];
    requestAll: boolean;
    approved: boolean;
    requested: boolean;
    available: boolean;
    plexUrl: string;
    embyUrl: string;
    quality: string;
    firstSeason: boolean;
    latestSeason: boolean;
    theTvDbId: string;
    subscribed: boolean;
    showSubscribe: boolean;
    fullyAvailable: boolean;
    partlyAvailable: boolean;
    network: INetwork;
    images: IImagesV2;
    cast: ICast[];
    crew: ICrew[];
}


export interface INetwork {
    id: number;
    name: string;
    country: ICountry;
}

export interface ICountry {
    name: string;
    code: string;
    timezone: string;
}

export interface IImagesV2 {
    medium: string;
    original: string;
}

export interface ICast {
    self: boolean;
    voide: boolean;
    person: IPersonViewModel;
    character: ICharacterViewModel;
}

export interface IPersonViewModel {
    id: number;
    url: string;
    name: string;
    image: IImagesV2;
}

export interface ICharacterViewModel {
    id: number;
    url: string;
    name: string;
    image: IImagesV2;
}

export interface ICrew {
    type: string;
    person: IPersonViewModel;
}