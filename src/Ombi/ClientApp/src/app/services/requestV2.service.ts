import { PlatformLocation, APP_BASE_HREF } from "@angular/common";
import { Injectable, Inject } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { ServiceHelpers } from "./service.helpers";
import { IRequestsViewModel, IMovieRequests, ITvRequests, IChildRequests } from "../interfaces";


@Injectable()
export class RequestServiceV2 extends ServiceHelpers {
    constructor(http: HttpClient, @Inject(APP_BASE_HREF) href:string) {
        super(http, "/api/v2/Requests/", href);
    }

    public getMovieRequests(count: number, position: number, sortProperty: string , order: string): Observable<IRequestsViewModel<IMovieRequests>> {
        return this.http.get<IRequestsViewModel<IMovieRequests>>(`${this.url}movie/${count}/${position}/${sortProperty}/${order}`, {headers: this.headers});
    }

    public getTvRequests(count: number, position: number, sortProperty: string , order: string): Observable<IRequestsViewModel<IChildRequests>> {
        return this.http.get<IRequestsViewModel<IChildRequests>>(`${this.url}tv/${count}/${position}/${sortProperty}/${order}`, {headers: this.headers});
    }

}
