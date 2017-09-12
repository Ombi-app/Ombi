import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Observable } from 'rxjs/Rx';

import { ServiceHelpers } from './service.helpers';
import { IMediaServerStatus } from '../interfaces/IMediaServerStatus';

@Injectable()
export class LandingPageService extends ServiceHelpers { 
    constructor(public http : Http) {
        super(http, '/api/v1/LandingPage/');
    }

    getServerStatus(): Observable<IMediaServerStatus> {
        return this.http.get(`${this.url}`, { headers: this.headers }).map(this.extractData);
    }
}