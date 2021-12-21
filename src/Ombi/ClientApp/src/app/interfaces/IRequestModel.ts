﻿import { IUser } from "./IUser";

export enum RequestType {
  tvShow = 0,
  movie = 1,
  artist = 2,
  album = 3
}

// NEW WORLD

export interface IMovieRequests extends IFullBaseRequest {
  theMovieDbId: number;
  rootPathOverride: number;
  qualityOverride: number;
  digitalReleaseDate: Date;
  subscribed: boolean;
  showSubscribe: boolean;
  requestStatus: string;

  // For the UI
  rootPathOverrideTitle: string;
  qualityOverrideTitle: string;
}

export interface IMovieAdvancedOptions {
  requestId: number;
  qualityOverride: number;
  rootPathOverride: number;
  languageProfile: number;
}

export interface IAlbumRequest extends IBaseRequest {
  foreignAlbumId: string;
  foreignArtistId: string;
  disk: string;
  cover: string;
  releaseDate: Date;
  artistName: string;

  subscribed: boolean;
  showSubscribe: boolean;
  background: any;
}

export interface IAlbumRequestModel {
  foreignAlbumId: string;
  monitored: boolean;
  monitor: string;
  searchForMissingAlbums: boolean;
}

export interface IArtistRequestModel {
  foreignArtistId: string;
  monitored: boolean;
  monitor: string;
  searchForMissingAlbums: boolean;
}

export interface IRequestsViewModel<T> {
  total: number;
  collection: T[];
}

export interface IMovieUpdateModel {
  id: number;
}

export interface IDenyMovieModel extends IMovieUpdateModel {
  reason: string;
}

export interface IAlbumUpdateModel {
  id: number;
}

export interface IDenyAlbumModel extends IAlbumUpdateModel {
  reason: string;
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
  requestedByAlias: string;
}

export interface ITvRequests {
  id: number;
  imdbId: string;
  externalProviderId: number;
  rootFolder: number;
  overview: string;
  title: string;
  posterPath: string;
  backdropPath: string;
  releaseDate: Date;
  status: string;
  childRequests: IChildRequests[];
  qualityOverride: number;
  languageProfile: number;
  background: any;
  totalSeasons: number;
  tvDbId: number; // NO LONGER USED

  open: boolean; // THIS IS FOR THE UI

  // For UI display
  qualityOverrideTitle: string;
  rootPathOverrideTitle: string;
  languageOverrideTitle: string;
}

export interface IChildRequests extends IBaseRequest {
  seasonRequests: INewSeasonRequests[];
  parentRequestId: number;
  parentRequest: ITvRequests;
  subscribed: boolean;
  showSubscribe: boolean;
  requestStatus: string;
}

export interface ITvUpdateModel {
  id: number;
}

export interface ITvDenyModel extends ITvUpdateModel {
  reason: string;
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
  overview: string;
  seasonNumber: number;
  episodes: IEpisodesRequests[];
  seasonAvailable: boolean;
}

export interface IEpisodesRequests {
  id: number;
  episodeNumber: number;
  title: string;
  airDate: Date;
  airDateDisplay: string;
  url: string;
  available: boolean;
  requested: boolean;
  approved: boolean;
  requestStatus: string;
  denied: boolean;
  selected: boolean; // This is for the UI only
}

export interface IMovieRequestModel extends BaseRequestOptions {
  theMovieDbId: number;
  languageCode: string | undefined;
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

export class BaseRequestOptions {
  requestOnBehalf: string | undefined;
  rootFolderOverride: number | undefined;
  qualityPathOverride: number | undefined;
}