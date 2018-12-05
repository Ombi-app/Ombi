import { PlatformLocation } from "@angular/common";
import { Injectable } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

import { IFailedRequestsViewModel } from "../interfaces";
import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class RequestRetryService extends ServiceHelpers {
    constructor(http: HttpClient, public platformLocation: PlatformLocation) {
        super(http, "/api/v1/requestretry/", platformLocation);
    }
    public getFailedRequests(): Observable<IFailedRequestsViewModel[]> {
        return this.http.get<IFailedRequestsViewModel[]>(this.url, {headers: this.headers});
    }
    public deleteFailedRequest(failedId: number): Observable<boolean> {
        return this.http.delete<boolean>(`${this.url}/${failedId}`, {headers: this.headers});
    }
}
