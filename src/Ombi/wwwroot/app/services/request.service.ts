import { Injectable } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';
import { Observable } from 'rxjs/Rx';

import { ServiceAuthHelpers } from './service.helpers';
import { IRequestEngineResult } from '../interfaces/IRequestEngineResult';
import { ISearchMovieResult } from '../interfaces/ISearchMovieResult';
import { ISearchTvResult } from '../interfaces/ISearchTvResult';
import { IMovieRequestModel, ITvRequestModel } from '../interfaces/IRequestModel';

@Injectable()
export class RequestService extends ServiceAuthHelpers {
    constructor(http: AuthHttp) {
        super(http, '/api/v1/Request/');
    }

    requestMovie(movie: ISearchMovieResult): Observable<IRequestEngineResult> {
        return this.http.post(`${this.url}Movie/`, JSON.stringify(movie), { headers: this.headers }).map(this.extractData);
    }

    requestTv(tv: ISearchTvResult): Observable<IRequestEngineResult> {
        return this.http.post(`${this.url}TV/`, JSON.stringify(tv), { headers: this.headers }).map(this.extractData);
    }

    getMovieRequests(count: number, position: number): Observable<IMovieRequestModel[]> {
        return this.http.get(`${this.url}movie/${count}/${position}`).map(this.extractData);
    }

    searchMovieRequests(search: string): Observable<IMovieRequestModel[]> {
        return this.http.get(`${this.url}movie/search/${search}`).map(this.extractData);
    }

    removeMovieRequest(request: IMovieRequestModel) {
        this.http.delete(`${this.url}movie/${request.id}`).map(this.extractData).subscribe();
    }

    updateMovieRequest(request: IMovieRequestModel): Observable<IMovieRequestModel> {
        return this.http.post(`${this.url}movie/`, JSON.stringify(request), { headers: this.headers }).map(this.extractData);
    }

    getTvRequests(count: number, position: number): Observable<ITvRequestModel[]> {
        return this.http.get(`${this.url}tv/${count}/${position}`).map(this.extractData);
    }

    searchTvRequests(search: string): Observable<ITvRequestModel[]> {
        return this.http.get(`${this.url}tv/search/${search}`).map(this.extractData);
    }

    removeTvRequest(request: ITvRequestModel) {
        this.http.delete(`${this.url}tv/${request.id}`).map(this.extractData).subscribe();
    }

    updateTvRequest(request: ITvRequestModel): Observable<ITvRequestModel> {
        return this.http.post(`${this.url}tv/`, JSON.stringify(request), { headers: this.headers }).map(this.extractData);
    }
}