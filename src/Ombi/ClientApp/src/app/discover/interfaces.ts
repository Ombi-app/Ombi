import { RequestType } from "../interfaces";

export interface IDiscoverCardResult {
    id: number;
    posterPath: string;
    url: string | undefined;
    title: string;
    type: RequestType;
    available: boolean;
    approved: boolean;
    requested: boolean;
    rating: number;
    overview: string;
    imdbid: string;
}

export enum DiscoverOption {
    Combined = 1,
    Movie = 2,
    Tv = 3
}

export enum DisplayOption {
    Card = 1,
    List = 2
}
