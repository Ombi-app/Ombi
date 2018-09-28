import { PlatformLocation } from "@angular/common";
import { Injectable } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class JobService extends ServiceHelpers {
    constructor(http: HttpClient, public platformLocation: PlatformLocation) {
        super(http, "/api/v1/Job/", platformLocation);
    }
    public forceUpdate(): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}update/`, {headers: this.headers});
    }

    public checkForNewUpdate(): Observable<boolean> {
        return this.http.get<boolean>(`${this.url}update/`, {headers: this.headers});
    }

    public getCachedUpdate(): Observable<boolean> {
        return this.http.get<boolean>(`${this.url}updateCached/`, {headers: this.headers});
    }

    public runPlexImporter(): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}plexUserImporter/`, {headers: this.headers});
    }

    public runEmbyImporter(): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}embyUserImporter/`, {headers: this.headers});
    }

    public runPlexCacher(): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}plexcontentcacher/`, {headers: this.headers});
    }

    public runPlexRecentlyAddedCacher(): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}plexrecentlyadded/`, {headers: this.headers});
    }

    public runEmbyCacher(): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}embycontentcacher/`, {headers: this.headers});
    }

    public runNewsletter(): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}newsletter/`, {headers: this.headers});
    }
}
