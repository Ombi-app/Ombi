import { PlatformLocation } from "@angular/common";
import { Injectable } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs/Rx";

import { IRecentlyAddedMovies, IRecentlyAddedRangeModel } from "./../interfaces";
import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class RecentlyAddedService extends ServiceHelpers {
    constructor(http: HttpClient, public platformLocation: PlatformLocation) {
        super(http, "/api/v1/recentlyadded/", platformLocation);
    }
    public getRecentlyAddedMovies(model: IRecentlyAddedRangeModel): Observable<IRecentlyAddedMovies[]> {
        return this.http.post<IRecentlyAddedMovies[]>(`${this.url}movies/`,JSON.stringify(model), {headers: this.headers});
    }
}
