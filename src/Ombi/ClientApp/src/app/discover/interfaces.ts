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