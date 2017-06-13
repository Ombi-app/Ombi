import { Injectable } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';
import { Observable } from 'rxjs/Rx';

import { ServiceAuthHelpers } from './service.helpers';
import { ISearchMovieResult } from '../interfaces/ISearchMovieResult';
import { ISearchTvResult } from '../interfaces/ISearchTvResult';

@Injectable()
export class SearchService extends ServiceAuthHelpers {
    constructor(http: AuthHttp) {
        super(http, "/api/v1/search");
    }

    // Movies
    searchMovie(searchTerm: string): Observable<ISearchMovieResult[]> {
        return this.http.get(`${this.url}/Movie/` + searchTerm).map(this.extractData);
    }

    popularMovies(): Observable<ISearchMovieResult[]> {
        return this.http.get(`${this.url}/Movie/Popular`).map(this.extractData);
    }
    upcomignMovies(): Observable<ISearchMovieResult[]> {
        return this.http.get(`${this.url}/Movie/upcoming`).map(this.extractData);
    }
    nowPlayingMovies(): Observable<ISearchMovieResult[]> {
        return this.http.get(`${this.url}/Movie/nowplaying`).map(this.extractData);
    }
    topRatedMovies(): Observable<ISearchMovieResult[]> {
        return this.http.get(`${this.url}/Movie/toprated`).map(this.extractData);
    }
    extraInfo(movies: ISearchMovieResult[]): Observable<ISearchMovieResult[]> {
        return this.http.post(`${this.url}/Movie/extrainfo`, JSON.stringify(movies), { headers: this.headers }).map(this.extractData);
    }
    getMovieInformation(theMovieDbId: number): Observable<ISearchMovieResult> {
        return this.http.get(`${this.url}/Movie/info/${theMovieDbId}`).map(this.extractData);
    }

    // TV
    searchTv(searchTerm: string): Observable<ISearchTvResult[]> {
        return this.http.get(`${this.url}/Tv/` + searchTerm).map(this.extractData);
    }

    getShowInformation(theTvDbId: number): Observable<ISearchTvResult> {
        return this.http.get(`${this.url}/Tv/info/${theTvDbId}`).map(this.extractData);
    }

    popularTv(): Observable<ISearchTvResult[]> {
        return this.http.get(`${this.url}/Tv/popular`).map(this.extractData);
    }
    mostWatchedTv(): Observable<ISearchTvResult[]> {
        return this.http.get(`${this.url}/Tv/mostwatched`).map(this.extractData);
    }
    anticiplatedTv(): Observable<ISearchTvResult[]> {
        return this.http.get(`${this.url}/Tv/anticipated`).map(this.extractData);
    }
    trendingTv(): Observable<ISearchTvResult[]> {
        return this.http.get(`${this.url}/Tv/trending`).map(this.extractData);
    }
}