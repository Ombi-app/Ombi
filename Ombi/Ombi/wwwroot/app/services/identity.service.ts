import { Injectable } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';
import { Http } from '@angular/http';
import { Observable } from 'rxjs/Rx';

import { ServiceAuthHelpers } from './service.helpers';
import {IUser} from '../interfaces/IUser';


@Injectable()
export class IdentityService extends ServiceAuthHelpers {
    constructor(http: AuthHttp, private regularHttp : Http) {
        super(http, '/api/v1/Identity/');
    }
    createWizardUser(username:string,password:string): Observable<boolean> {
        return this.regularHttp.post(`${this.url}/Wizard/`, JSON.stringify({username:username, password:password}), { headers: this.headers }).map(this.extractData);
    }

    getUsers(): Observable<IUser[]> {
        return this.http.get(`${this.url}/Users`).map(this.extractData);
    }
}