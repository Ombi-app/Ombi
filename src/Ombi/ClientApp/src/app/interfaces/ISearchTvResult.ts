import { INewSeasonRequests } from "./IRequestModel";

export interface ISearchTvResult {
    id: number;
    title: string; // used in the request
    aliases: string[];
    banner: string;
    seriesId: number;
    status: string;
    firstAired: string;
    network: string;
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
    jellyfinUrl: string;
    quality: string;
    firstSeason: boolean;
    latestSeason: boolean;
    theTvDbId: string;
    subscribed: boolean;
    showSubscribe: boolean;
    fullyAvailable: boolean;
    partlyAvailable: boolean;
    background: any;
    open: boolean; // THIS IS FOR THE UI
}

export interface ITvRequestViewModel {
    requestAll: boolean;
    firstSeason: boolean;
    latestSeason: boolean;
    tvDbId: number;
    seasons: ISeasonsViewModel[];
    requestOnBehalf: string | undefined;
}

export interface ISeasonsViewModel {
    seasonNumber: number;
    episodes: IEpisodesViewModel[];
}

export interface IEpisodesViewModel {
    episodeNumber: number;
}
