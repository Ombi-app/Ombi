import { PlatformLocation } from "@angular/common";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";

import { HttpClient } from "@angular/common/http";

import { IImages } from "../interfaces";
import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class ImageService extends ServiceHelpers {
    constructor(public http: HttpClient, public platformLocation: PlatformLocation) {
        super(http, "/api/v1/Images/", platformLocation);
    }

    public getRandomBackground(): Observable<IImages> {
        return this.http.get<IImages>(`${this.url}background/`, {headers: this.headers});
    }

    public getTvBanner(tvdbid: number): Observable<string> {
        return this.http.get<string>(`${this.url}tv/${tvdbid}`, {headers: this.headers});
    }

    public getMoviePoster(movieDbId: string): Observable<string> {
        return this.http.get<string>(`${this.url}poster/movie/${movieDbId}`, { headers: this.headers });
    }

    public getTvPoster(tvdbid: number): Observable<string> {
        return this.http.get<string>(`${this.url}poster/tv/${tvdbid}`, { headers: this.headers });
    }

    public getMovieBackground(movieDbId: string): Observable<string> {
        return this.http.get<string>(`${this.url}background/movie/${movieDbId}`, { headers: this.headers });
    }

    public getTvBackground(tvdbid: number): Observable<string> {
        return this.http.get<string>(`${this.url}background/tv/${tvdbid}`, { headers: this.headers });
    }

}
