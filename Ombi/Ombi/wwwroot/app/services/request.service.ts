import { Injectable } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';
import { Observable } from 'rxjs/Rx';

import { ServiceAuthHelpers } from './service.helpers';
import { IRequestEngineResult } from '../interfaces/IRequestEngineResult';
import { ISearchMovieResult } from '../interfaces/ISearchMovieResult';
import { IRequestModel } from '../interfaces/IRequestModel';

@Injectable()
export class RequestService extends ServiceAuthHelpers {
    constructor(http: AuthHttp) {
        super(http, '/api/v1/Request/');
    }

    requestMovie(movie: ISearchMovieResult): Observable<IRequestEngineResult> {
        return this.http.post(`${this.url}/Movie/`, JSON.stringify(movie), { headers: this.headers }).map(this.extractData);
    }

    getAllRequests(): Observable<IRequestModel[]> {
        return this.http.get(this.url).map(this.extractData);
    }

    getRequests(count: number, position: number): Observable<IRequestModel[]> {
        return this.http.get(`${this.url}/${count}/${position}`).map(this.extractData);
    }

    searchRequests(search: string): Observable<IRequestModel[]> {
        return this.http.get(`${this.url}/search/${search}`).map(this.extractData);
    }

    removeRequest(request: IRequestModel): Observable<void> {
        return this.http.delete(`${this.url}/${request.id}`).map(this.extractData);
    }

    updateRequest(request: IRequestModel) : Observable<IRequestModel> {
        return this.http.post(`${this.url}/`, JSON.stringify(request), { headers: this.headers }).map(this.extractData);
    }
}