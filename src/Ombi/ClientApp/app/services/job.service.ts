import { PlatformLocation } from "@angular/common";
import { Injectable } from "@angular/core";
import { AuthHttp } from "angular2-jwt";
import { Observable } from "rxjs/Rx";

import { ServiceAuthHelpers } from "./service.helpers";

@Injectable()
export class JobService extends ServiceAuthHelpers {
    constructor(http: AuthHttp, public platformLocation: PlatformLocation) {
        super(http, "/api/v1/Job/", platformLocation);
    }
    public forceUpdate(): Observable<boolean> {
        return this.http.post(`${this.url}update/`, { headers: this.headers }).map(this.extractData);
    }

    public checkForNewUpdate(): Observable<boolean> {
        return this.http.get(`${this.url}update/`).map(this.extractData);
    }

    public getCachedUpdate(): Observable<boolean> {
        return this.http.get(`${this.url}updateCached/`).map(this.extractData);
    }

    public runPlexImporter(): Observable<boolean> {
        return this.http.post(`${this.url}plexUserImporter/`, { headers: this.headers }).map(this.extractData);
    }

    public runEmbyImporter(): Observable<boolean> {
        return this.http.post(`${this.url}embyUserImporter/`, { headers: this.headers }).map(this.extractData);
    }

    public runPlexCacher(): Observable<boolean> {
        return this.http.post(`${this.url}plexcontentcacher/`, { headers: this.headers }).map(this.extractData);
    }

    public runEmbyCacher(): Observable<boolean> {
        return this.http.post(`${this.url}embycontentcacher/`, { headers: this.headers }).map(this.extractData);
    }
}
