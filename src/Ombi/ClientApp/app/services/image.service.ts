import { PlatformLocation } from "@angular/common";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs/Rx";

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
    public getMoviePoster(themoviedbid: string): Observable<string> {
        return this.http.get<string>(`${this.url}poster/movie/${themoviedbid}`, {headers: this.headers});
    }
    public getTvPoster(tvdbid: number): Observable<string> {
        return this.http.get<string>(`${this.url}poster/tv/${tvdbid}`, {headers: this.headers});
    }
}
