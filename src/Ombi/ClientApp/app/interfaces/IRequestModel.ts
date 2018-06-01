﻿import { IUser } from "./IUser";

export enum RequestType {
  movie = 1,
  tvShow = 2,
}

// NEW WORLD

export interface IMovieRequests extends IFullBaseRequest {
  theMovieDbId: number;
  rootPathOverride: number;
  qualityOverride: number;
  digitalReleaseDate: Date;
  subscribed: boolean;
  showSubscribe: boolean;

  // For the UI
  rootPathOverrideTitle: string;
  qualityOverrideTitle: string;
}

export interface IRequestsViewModel<T> {
  total: number;
  collection: T[];
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
  title: string;
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
  qualityOverride: number;

  // For UI display
  qualityOverrideTitle: string;
  rootPathOverrideTitle: string;
}

export interface IChildRequests extends IBaseRequest {
  seasonRequests: INewSeasonRequests[];
  subscribed: boolean;
  showSubscribe: boolean;
}

export interface ITvUpdateModel {
  id: number;
}

export enum OrderType {
  RequestedDateAsc = 1,
  RequestedDateDesc = 2,
  TitleAsc = 3,
  TitleDesc = 4,
  StatusAsc = 5,
  StatusDesc = 6,
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

export interface IMovieRequestModel {
  theMovieDbId: number;
}

export interface IFilter {
  availabilityFilter: FilterType;
  statusFilter: FilterType;
}

export enum FilterType {
  None = 0,
  Available = 1,
  NotAvailable = 2,
  Approved = 3,
  Processing = 4,
  PendingApproval = 5,
}
