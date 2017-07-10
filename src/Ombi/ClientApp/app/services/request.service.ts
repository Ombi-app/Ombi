import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { AuthHttp } from 'angular2-jwt';
import { Observable } from 'rxjs/Rx';

import { ServiceAuthHelpers } from './service.helpers';
import { IRequestEngineResult } from '../interfaces/IRequestEngineResult';
import { ISearchMovieResult } from '../interfaces/ISearchMovieResult';
import { ISearchTvResult } from '../interfaces/ISearchTvResult';
import { IMovieRequests, ITvRequests, IRequestCountModel, IRequestGrid, IChildRequests } from '../interfaces/IRequestModel';

@Injectable()
export class RequestService extends ServiceAuthHelpers {
    constructor(http: AuthHttp, private basicHttp : Http) {
        super(http, '/api/v1/Request/');
    }

    requestMovie(movie: ISearchMovieResult): Observable<IRequestEngineResult> {
        return this.http.post(`${this.url}Movie/`, JSON.stringify(movie), { headers: this.headers }).map(this.extractData);
    }

    requestTv(tv: ISearchTvResult): Observable<IRequestEngineResult> {
        return this.http.post(`${this.url}TV/`, JSON.stringify(tv), { headers: this.headers }).map(this.extractData);
    }

    getMovieRequests(count: number, position: number): Observable<IMovieRequests[]> {
        return this.http.get(`${this.url}movie/${count}/${position}`).map(this.extractData);
    }

    searchMovieRequests(search: string): Observable<IMovieRequests[]> {
        return this.http.get(`${this.url}movie/search/${search}`).map(this.extractData);
    }

    removeMovieRequest(request: IMovieRequests) {
        this.http.delete(`${this.url}movie/${request.id}`).map(this.extractData).subscribe();
    }

    updateMovieRequest(request: IMovieRequests): Observable<IMovieRequests> {
        return this.http.post(`${this.url}movie/`, JSON.stringify(request), { headers: this.headers }).map(this.extractData);
    }

    getTvRequests(count: number, position: number): Observable<ITvRequests[]> {
        return this.http.get(`${this.url}tv/${count}/${position}`).map(this.extractData)
            .catch(this.handleError);
    }   
    
     getChildRequests(requestId: number): Observable<IChildRequests[]> {
        return this.http.get(`${this.url}tv/${requestId}/child`).map(this.extractData)
            .catch(this.handleError);
    }

    searchTvRequests(search: string): Observable<ITvRequests[]> {
        return this.http.get(`${this.url}tv/search/${search}`).map(this.extractData);
    }

    removeTvRequest(request: ITvRequests) {
        this.http.delete(`${this.url}tv/${request.id}`).map(this.extractData).subscribe();
    }

    updateTvRequest(request: ITvRequests): Observable<ITvRequests> {
        return this.http.put(`${this.url}tv/`, JSON.stringify(request), { headers: this.headers }).map(this.extractData);
    }

    getRequestsCount(): Observable<IRequestCountModel> {
        return this.basicHttp.get(`${this.url}count`).map(this.extractData);
    }

    getMovieGrid(): Observable<IRequestGrid<IMovieRequests>> {
        return this.http.get(`${this.url}movie/grid`).map(this.extractData);
    }

    getTvGrid(): Observable<IRequestGrid<ITvRequests>> {
        return this.http.get(`${this.url}tv/grid`).map(this.extractData);
    }
}