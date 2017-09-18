﻿import { ISeasonRequests } from "./IRequestModel";

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
    seasonsRequests: ISeasonRequests[];
    requestAll: boolean;
    approved: boolean;
    requested: boolean;
    available: boolean;
    plexUrl: string;
    firstSeason: boolean;
    latestSeason: boolean;
}
