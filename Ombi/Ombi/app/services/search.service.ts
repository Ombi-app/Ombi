import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Observable } from 'rxjs/Rx';

import { ServiceHelpers } from './service.helpers';
import { ISearchMovieResult } from '../interfaces/ISearchMovieResult';

@Injectable()
export class SearchService {
    constructor(private http: Http) {
    }

    searchMovie(searchTerm: string): Observable<ISearchMovieResult[]> {
        return this.http.get('/api/Search/Movie/' + searchTerm).map(ServiceHelpers.extractData);
    }

    popularMovies(): Observable<ISearchMovieResult[]> {
        return this.http.get('/api/Search/Movie/Popular').map(ServiceHelpers.extractData);
    }
    upcomignMovies(): Observable<ISearchMovieResult[]> {
        return this.http.get('/api/Search/Movie/upcoming').map(ServiceHelpers.extractData);
    }
    nowPlayingMovies(): Observable<ISearchMovieResult[]> {
        return this.http.get('/api/Search/Movie/nowplaying').map(ServiceHelpers.extractData);
    }
    topRatedMovies(): Observable<ISearchMovieResult[]> {
        return this.http.get('/api/Search/Movie/toprated').map(ServiceHelpers.extractData);
    }
}