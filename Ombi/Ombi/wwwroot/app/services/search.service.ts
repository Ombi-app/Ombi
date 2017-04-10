import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Observable } from 'rxjs/Rx';

import { ServiceHelpers } from './service.helpers';
import { ISearchMovieResult } from '../interfaces/ISearchMovieResult';

@Injectable()
export class SearchService extends ServiceHelpers {
    constructor(http: Http) {
        super(http, "/api/v1/search");
    }

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
}