import { PlatformLocation, APP_BASE_HREF } from "@angular/common";
import { Injectable, Inject } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

import { IRecentlyAddedMovies, IRecentlyAddedTvShows } from "../interfaces";
import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class RecentlyAddedService extends ServiceHelpers {
    constructor(http: HttpClient, @Inject(APP_BASE_HREF) href:string) {
        super(http, "/api/v1/recentlyadded/", href);
    }
    public getRecentlyAddedMovies(): Observable<IRecentlyAddedMovies[]> {
        return this.http.get<IRecentlyAddedMovies[]>(`${this.url}movies/`, {headers: this.headers});
    }

    public getRecentlyAddedTv(): Observable<IRecentlyAddedTvShows[]> {
        return this.http.get<IRecentlyAddedTvShows[]>(`${this.  url}tv/`, {headers: this.headers});
    }

    public getRecentlyAddedTvGrouped(): Observable<IRecentlyAddedTvShows[]> {
        return this.http.get<IRecentlyAddedTvShows[]>(`${this.url}tv/grouped`, {headers: this.headers});
    }
}
