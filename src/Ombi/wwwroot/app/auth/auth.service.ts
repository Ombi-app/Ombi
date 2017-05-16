
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Rx';

import { ServiceHelpers } from '../services/service.helpers';

import { IUserLogin } from './IUserLogin';

import { tokenNotExpired } from 'angular2-jwt';

import { Http } from '@angular/http';

@Injectable()
export class AuthService extends ServiceHelpers {
    constructor(http: Http) {
        super(http, '/api/v1/token');
    }

    login(login:IUserLogin) : Observable<any> {
        return this.http.post(`${this.url}/`, JSON.stringify(login), { headers: this.headers })
            .map(this.extractData);

    }

    loggedIn() {
        return tokenNotExpired('id_token');
    }

    logout() {
        localStorage.removeItem('id_token');
        localStorage.removeItem('currentUser');
    }
}

