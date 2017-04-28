import { Injectable } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';
import { Observable } from 'rxjs/Rx';

import { ServiceAuthHelpers } from './service.helpers';
import { IRequestEngineResult } from '../interfaces/IRequestEngineResult';
import { ISearchMovieResult } from '../interfaces/ISearchMovieResult';
import { ISearchTvResult } from '../interfaces/ISearchTvResult';
import { IMovieRequestModel } from '../interfaces/IRequestModel';

@Injectable()
export class RequestService extends ServiceAuthHelpers {
    constructor(http: AuthHttp) {
        super(http, '/api/v1/Request/');
    }

    requestMovie(movie: ISearchMovieResult): Observable<IRequestEngineResult> {
        return this.http.post(`${this.url}/Movie/`, JSON.stringify(movie), { headers: this.headers }).map(this.extractData);
    }

    requestTv(tv: ISearchTvResult): Observable<IRequestEngineResult> {
        return this.http.post(`${this.url}/TV/`, JSON.stringify(tv), { headers: this.headers }).map(this.extractData);
    }

    getRequests(count: number, position: number): Observable<IMovieRequestModel[]> {
        return this.http.get(`${this.url}/movie/${count}/${position}`).map(this.extractData);
    }

    searchRequests(search: string): Observable<IMovieRequestModel[]> {
        return this.http.get(`${this.url}/movie/search/${search}`).map(this.extractData);
    }

    removeMovieRequest(request: IMovieRequestModel) {
        this.http.delete(`${this.url}/movie/${request.id}`).map(this.extractData).subscribe();
    }

    updateRequest(request: IMovieRequestModel): Observable<IMovieRequestModel> {
        return this.http.post(`${this.url}/movie/`, JSON.stringify(request), { headers: this.headers }).map(this.extractData);
    }
}