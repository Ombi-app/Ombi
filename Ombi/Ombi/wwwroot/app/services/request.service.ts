import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Observable } from 'rxjs/Rx';

import { ServiceHelpers } from './service.helpers';
import { IRequestEngineResult } from '../interfaces/IRequestEngineResult';
import { ISearchMovieResult } from '../interfaces/ISearchMovieResult';
import { IRequestModel } from '../interfaces/IRequestModel';

@Injectable()
export class RequestService {
    constructor(private http: Http) {
    }

    requestMovie(movie: ISearchMovieResult): Observable<IRequestEngineResult> {
        return this.http.post('/api/Request/Movie/', JSON.stringify(movie), ServiceHelpers.RequestOptions).map(ServiceHelpers.extractData);
    }

    getAllRequests(): Observable<IRequestModel[]> {
        return this.http.get('/api/request').map(ServiceHelpers.extractData);
    }

}