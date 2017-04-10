import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Observable } from 'rxjs/Rx';

import { ServiceHelpers } from './service.helpers';
import { IRequestEngineResult } from '../interfaces/IRequestEngineResult';
import { ISearchMovieResult } from '../interfaces/ISearchMovieResult';
import { IRequestModel } from '../interfaces/IRequestModel';

@Injectable()
export class RequestService extends ServiceHelpers {
    constructor(http: Http) {
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