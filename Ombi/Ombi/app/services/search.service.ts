import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Observable } from 'rxjs/Rx';

import { ServiceHelpers } from './service.helpers';
import { IMovieResult } from '../search/interfaces/IMovieResult';

@Injectable()
export class SearchService {
    constructor(private http: Http) {
    }

    searchMovie(searchTerm: string): Observable<IMovieResult[]> {
        return this.http.get('/api/Search/Movie/' + searchTerm).map(ServiceHelpers.extractData);
    }
}