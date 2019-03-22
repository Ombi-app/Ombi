import { PlatformLocation, APP_BASE_HREF } from "@angular/common";
import { Injectable, Inject } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

import { IMultiSearchResult, ISearchMovieResult, ISearchTvResult } from "../interfaces";
import { ServiceHelpers } from "./service.helpers";

import { ISearchMovieResultV2 } from "../interfaces/ISearchMovieResultV2";
import { promise } from "selenium-webdriver";
import { ISearchTvResultV2 } from "../interfaces/ISearchTvResultV2";

@Injectable()
export class SearchV2Service extends ServiceHelpers {
    constructor(http: HttpClient, @Inject(APP_BASE_HREF) href:string) {
        super(http, "/api/v2/search", href);
    }

    public multiSearch(searchTerm: string): Observable<IMultiSearchResult[]> {
        return this.http.get<IMultiSearchResult[]>(`${this.url}/multi/${searchTerm}`);
    }
    public getFullMovieDetails(theMovieDbId: number): Observable<ISearchMovieResultV2> {
        return this.http.get<ISearchMovieResultV2>(`${this.url}/Movie/${theMovieDbId}`);
    }
    public getFullMovieDetailsPromise(theMovieDbId: number): Promise<ISearchMovieResultV2> {
        return this.http.get<ISearchMovieResultV2>(`${this.url}/Movie/${theMovieDbId}`).toPromise();
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
    
    public getTvInfo(tvdbid: number): Promise<ISearchTvResultV2> {
        return this.http.get<ISearchTvResultV2>(`${this.url}/Tv/${tvdbid}`, { headers: this.headers }).toPromise();
    }
    public getTvInfoWithMovieDbId(theMovieDbId: number): Promise<ISearchTvResultV2> {
        return this.http.get<ISearchTvResultV2>(`${this.url}/Tv/moviedb/${theMovieDbId}`, { headers: this.headers }).toPromise();
    }
}
