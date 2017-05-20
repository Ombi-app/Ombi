import { Injectable } from '@angular/core';
import { Http } from '@angular/http'

import { AuthHttp } from 'angular2-jwt';;
import { Observable } from 'rxjs/Rx';

import { ServiceAuthHelpers } from '../service.helpers';

import { IPlexAuthentication, IPlexLibraries } from '../../interfaces/IPlex';
import { IPlexServer } from '../../interfaces/ISettings';


@Injectable()
export class PlexService extends ServiceAuthHelpers {
    constructor(http: AuthHttp, private regularHttp: Http) {
        super(http, '/api/v1/Plex/');
    }

    logIn(login: string, password: string): Observable<IPlexAuthentication> {
        return this.regularHttp.post(`${this.url}`, JSON.stringify({ login: login, password:password}), { headers: this.headers }).map(this.extractData);
    }

    getLibraries(plexSettings: IPlexServer): Observable<IPlexLibraries> {
        return this.http.post(`${this.url}Libraries`, JSON.stringify(plexSettings), { headers: this.headers }).map(this.extractData);
    }
    
}