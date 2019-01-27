import { RequestType } from "../interfaces";

export interface IDiscoverCardResult {
    id: number;
    posterPath: string;
    url: string | undefined;
    title: string;
    type: RequestType;
    available: boolean;
    requested: boolean;
    rating: number;
}