import { PlatformLocation } from "@angular/common";
import { Injectable } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { ServiceHelpers } from "./service.helpers";
import { IRequestsViewModel, IMovieRequests, ITvRequests } from "../interfaces";


@Injectable()
export class RequestServiceV2 extends ServiceHelpers {
    constructor(http: HttpClient, public platformLocation: PlatformLocation) {
        super(http, "/api/v2/Requests/", platformLocation);
    }

    public getMovieRequests(count: number, position: number, sortProperty: string , order: string): Observable<IRequestsViewModel<IMovieRequests>> {
        return this.http.get<IRequestsViewModel<IMovieRequests>>(`${this.url}movie/${count}/${position}/${sortProperty}/${order}`, {headers: this.headers});
    }

    public getTvRequests(count: number, position: number, sortProperty: string , order: string): Observable<IRequestsViewModel<ITvRequests>> {
        return this.http.get<IRequestsViewModel<ITvRequests>>(`${this.url}tv/${count}/${position}/${sortProperty}/${order}`, {headers: this.headers});
    }

}
