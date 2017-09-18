import { Injectable } from "@angular/core";
import { AuthHttp } from "angular2-jwt";
import { Observable } from "rxjs/Rx";

import { ISearchMovieResult } from "../interfaces";
import { ISearchTvResult } from "../interfaces";
import { ServiceAuthHelpers } from "./service.helpers";

@Injectable()
export class SearchService extends ServiceAuthHelpers {
    constructor(http: AuthHttp) {
        super(http, "/api/v1/search");
    }

    // Movies
    public searchMovie(searchTerm: string): Observable<ISearchMovieResult[]> {
        return this.http.get(`${this.url}/Movie/` + searchTerm).map(this.extractData);
    }

    public popularMovies(): Observable<ISearchMovieResult[]> {
        return this.http.get(`${this.url}/Movie/Popular`).map(this.extractData);
    }
    public upcomignMovies(): Observable<ISearchMovieResult[]> {
        return this.http.get(`${this.url}/Movie/upcoming`).map(this.extractData);
    }
    public nowPlayingMovies(): Observable<ISearchMovieResult[]> {
        return this.http.get(`${this.url}/Movie/nowplaying`).map(this.extractData);
    }
    public topRatedMovies(): Observable<ISearchMovieResult[]> {
        return this.http.get(`${this.url}/Movie/toprated`).map(this.extractData);
    }
    public getMovieInformation(theMovieDbId: number): Observable<ISearchMovieResult> {
        return this.http.get(`${this.url}/Movie/info/${theMovieDbId}`).map(this.extractData);
    }

    // TV
    public searchTv(searchTerm: string): Observable<ISearchTvResult[]> {
        return this.http.get(`${this.url}/Tv/` + searchTerm).map(this.extractData);
    }

    public getShowInformation(theTvDbId: number): Observable<ISearchTvResult> {
        return this.http.get(`${this.url}/Tv/info/${theTvDbId}`).map(this.extractData);
    }

    public popularTv(): Observable<ISearchTvResult[]> {
        return this.http.get(`${this.url}/Tv/popular`).map(this.extractData);
    }
    public mostWatchedTv(): Observable<ISearchTvResult[]> {
        return this.http.get(`${this.url}/Tv/mostwatched`).map(this.extractData);
    }
    public anticipatedTv(): Observable<ISearchTvResult[]> {
        return this.http.get(`${this.url}/Tv/anticipated`).map(this.extractData);
    }
    public trendingTv(): Observable<ISearchTvResult[]> {
        return this.http.get(`${this.url}/Tv/trending`).map(this.extractData);
    }
}
