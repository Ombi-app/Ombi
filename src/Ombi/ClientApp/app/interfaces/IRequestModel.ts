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

//export interface IMovieRequestModel extends IMediaBase { }

export interface ITvRequestModel extends IMediaBase {
  imdbId: string;
  tvDbId: string;
  childRequests: IChildTvRequest[];
  rootFolderSelected: number;
  firstAired: string;
}

export interface IRequestCountModel {
  pending: number;
  approved: number;
  available: number;
}

export interface IChildTvRequest extends IMediaBase {
  requestAll: boolean;
  seasonRequests: ISeasonRequests[];
}

export interface ISeasonRequests {
  seasonNumber: number;
  episodes: IEpisodesRequested[];
}

export interface IEpisodesRequested {
  episodeNumber: number;
  title: string;
  airDate: Date;
  url: string;
  requested: boolean;
  status: string;
  available: boolean;
}

export enum RequestType {
  movie = 1,
  tvShow = 2
}

export interface IRequestsPageScroll {
  count: number;
  position: number;
}

export interface IRequestGrid<T> {
  available: T[];
  new: T[];
  approved: T[];
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
  releaseDate: Date;
  status: string;
  released: boolean;
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
  releaseDate: Date;
  status: string;
  childRequests: IChildRequests[];
}

export interface IChildRequests extends IBaseRequest {
  seasonRequests: INewSeasonRequests[];
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
