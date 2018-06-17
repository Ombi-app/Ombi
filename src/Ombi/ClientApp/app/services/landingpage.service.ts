import { PlatformLocation } from "@angular/common";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";

import { HttpClient } from "@angular/common/http";

import { IMediaServerStatus } from "../interfaces";
import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class LandingPageService extends ServiceHelpers {
    constructor(public http: HttpClient, public platformLocation: PlatformLocation) {
        super(http, "/api/v1/LandingPage/", platformLocation);
    }

    public getServerStatus(): Observable<IMediaServerStatus> {
        return this.http.get<IMediaServerStatus>(`${this.url}`, {headers: this.headers});
    }
}
