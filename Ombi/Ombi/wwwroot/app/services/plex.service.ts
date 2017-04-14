import { Injectable } from '@angular/core';
import {  Http } from '@angular/http';
import { Observable } from 'rxjs/Rx';

import { ServiceHelpers } from './service.helpers';

import { IPlexAuthentication } from '../interfaces/IPlex'


@Injectable()
export class PlexService extends ServiceHelpers {
    constructor(http: Http) {
        super(http, '/api/v1/Plex/');
    }

    logIn(login: string, password: string): Observable<IPlexAuthentication> {
        return this.http.post(`${this.url}/`, JSON.stringify({ login: login, password:password}), { headers: this.headers }).map(this.extractData);
    }
    
}