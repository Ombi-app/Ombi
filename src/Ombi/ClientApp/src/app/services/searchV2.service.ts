import { PlatformLocation } from "@angular/common";
import { Injectable } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

import { IMultiSearchResult, ISearchMovieResult, ISearchTvResult } from "../interfaces";
import { ServiceHelpers } from "./service.helpers";
+
import { ISearchMovieResultV2 } from "../interfaces/ISearchMovieResultV2";

@Injectable()
export class SearchV2Service extends ServiceHelpers {
    constructor(http: HttpClient, public platformLocation: PlatformLocation) {
        super(http, "/api/v2/search", platformLocation);
    }

    public multiSearch(searchTerm: string): Observable<IMultiSearchResult[]> {
        return this.http.get<IMultiSearchResult[]>(`${this.url}/multi/${searchTerm}`);
    }
    public getFullMovieDetails(theMovieDbId: number): Observable<ISearchMovieResultV2> {
        return this.http.get<ISearchMovieResultV2>(`${this.url}/Movie/${theMovieDbId}`);
    }
    
    public similarMovies(theMovieDbId: number, langCode: string): Observable<ISearchMovieResult[]> {
        return this.http.post<ISearchMovieResult[]>(`${this.url}/Movie/similar`, {theMovieDbId, languageCode: langCode});
    }

    public popularMovies(): Observable<ISearchMovieResult[]> {
        return this.http.get<ISearchMovieResult[]>(`${this.url}/Movie/Popular`);
    }

    public upcomingMovies(): Observable<ISearchMovieResult[]> {
        return this.http.get<ISearchMovieResult[]>(`${this.url}/Movie/upcoming`);
    }

    public nowPlayingMovies(): Observable<ISearchMovieResult[]> {
        return this.http.get<ISearchMovieResult[]>(`${this.url}/Movie/nowplaying`);
    }
    
    public topRatedMovies(): Observable<ISearchMovieResult[]> {
        return this.http.get<ISearchMovieResult[]>(`${this.url}/Movie/toprated`);
    }
    
    public popularTv(): Observable<ISearchTvResult[]> {
        return this.http.get<ISearchTvResult[]>(`${this.url}/Tv/popular`, { headers: this.headers });
    }
    public mostWatchedTv(): Observable<ISearchTvResult[]> {
        return this.http.get<ISearchTvResult[]>(`${this.url}/Tv/mostwatched`, { headers: this.headers });
    }
    public anticipatedTv(): Observable<ISearchTvResult[]> {
        return this.http.get<ISearchTvResult[]>(`${this.url}/Tv/anticipated`, { headers: this.headers });
    }
    public trendingTv(): Observable<ISearchTvResult[]> {
        return this.http.get<ISearchTvResult[]>(`${this.url}/Tv/trending`, { headers: this.headers });
    }
}
