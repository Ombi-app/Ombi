import { PlatformLocation, APP_BASE_HREF } from "@angular/common";
import { Injectable, Inject } from "@angular/core";
import { Observable } from "rxjs";

import { HttpClient } from "@angular/common/http";

import { IMediaServerStatus } from "../interfaces";
import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class LandingPageService extends ServiceHelpers {
    constructor(public http: HttpClient, @Inject(APP_BASE_HREF) href:string) {
        super(http, "/api/v1/LandingPage/", href);
    }

    public getServerStatus(): Observable<IMediaServerStatus> {
        return this.http.get<IMediaServerStatus>(`${this.url}`, {headers: this.headers});
    }
}
