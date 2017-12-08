import { IUser } from "./IUser";

export interface IMediaBase {
  imdbId: string;
  id: number;
  providerId: number;
  title: string;
  overview: string;
  posterPath: string;
  releaseDate: Date;
  status: string;
  requestedDate: Date;
  approved: boolean;
  type: RequestType;
  requested: boolean;
  available: boolean;
  otherMessage: string;
  adminNote: string;
  requestedUser: string;
  issueId: number;
  denied: boolean;
  deniedReason: string;
  released: boolean;
}

export enum RequestType {
  movie = 1,
  tvShow = 2,
}

// NEW WORLD

export interface IMovieRequests extends IFullBaseRequest {
  theMovieDbId: number;
  rootPathOverride: number;
  qualityOverride: number;

  rootPathOverrideTitle: string;
  qualityOverrideTitle: string;
}

export interface IMovieUpdateModel {
  id: number;
}

export interface IFullBaseRequest extends IBaseRequest {
  imdbId: string;
  overview: string;
  title: string;
  posterPath: string;
  backdropPath: string;
  releaseDate: Date;
  status: string;
  released: boolean;
  background: string;

  // Used in the UI
  backgroundPath: any;
}

export interface IBaseRequest {
  id: number;
  approved: boolean;
  requestedDate: Date;
  available: boolean;
  requestedUserId: number;
  issueId: number;
  denied: boolean;
  deniedReason: string;
  requestType: RequestType;
  requestedUser: IUser;
  canApprove: boolean;
}

export interface ITvRequests {
  id: number;
  imdbId: string;
  rootFolder: number;
  overview: string;
  title: string;
  posterPath: string;
  backdropPath: string;
  releaseDate: Date;
  status: string;
  childRequests: IChildRequests[];
}

export interface IChildRequests extends IBaseRequest {
  seasonRequests: INewSeasonRequests[];
}

export interface ITvUpdateModel {
  id: number;
}

export interface INewSeasonRequests {
  id: number;
  seasonNumber: number;
  episodes: IEpisodesRequests[];
}

export interface IEpisodesRequests {
  id: number;
  episodeNumber: number;
  title: string;
  airDate: Date;
  url: string;
  available: boolean;
  requested: boolean;
  approved: boolean;
  selected: boolean; // This is for the UI only
}
