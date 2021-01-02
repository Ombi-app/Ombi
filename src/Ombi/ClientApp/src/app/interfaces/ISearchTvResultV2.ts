import { INewSeasonRequests, RequestType } from "./IRequestModel";

export interface ISearchTvResultV2 {
    id: number;
    title: string; // used in the request
    aliases: string[];
    background: any;
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
    certification: string;
    seasonRequests: INewSeasonRequests[];
    requestAll: boolean;
    approved: boolean;
    requested: boolean;
    available: boolean;
    plexUrl: string;
    embyUrl: string;
    jellyfinUrl: string;
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
    requestId: number;
}

export interface IMovieCollectionsViewModel {
    name: string;
    overview: string;
    collection: IMovieCollection[];
}

export interface IMovieCollection {
    id: number;
    overview: string;
    posterPath: string;
    title: string;
    type: RequestType;

    approved: boolean;
    requested: boolean;
    available: boolean;
    plexUrl: string;
    embyUrl: string; 
    jellyfinUrl: string; 
    imdbId: string;
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

export interface IActorCredits {
    cast: IActorCast[];
    crew: IActorCrew[];
}

export interface IActorCast {
    character: string;
    poster_path: string;
    id: number;
    backdrop_path: string;
    title: string;
    overview: string;
    release_date: Date;
}

export interface IActorCrew {
    id: number;
    department: string;
    job: string;
    overview: string;
    release_date: Date;
    title: string;
    backdrop_path: string;
    poster_path: string;
    credit_id: number;
}
