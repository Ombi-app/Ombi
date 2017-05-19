
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Rx';

import { ServiceHelpers } from '../services/service.helpers';

import { IUserLogin, ILocalUser } from './IUserLogin';

import { tokenNotExpired, JwtHelper } from 'angular2-jwt';

import { Http } from '@angular/http';

@Injectable()
export class AuthService extends ServiceHelpers {
    constructor(http: Http) {
        super(http, '/api/v1/token');
    }

    jwtHelper: JwtHelper = new JwtHelper();

    login(login:IUserLogin) : Observable<any> {
        return this.http.post(`${this.url}/`, JSON.stringify(login), { headers: this.headers })
            .map(this.extractData);

    }

    loggedIn() {
        return tokenNotExpired('id_token');
    }

    claims(): ILocalUser {
        if (this.loggedIn()) {
            var token = localStorage.getItem('id_token');

            var json = this.jwtHelper.decodeToken(token);
            var roles = json["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]
            var name = json["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"];


            var u = { name: name, roles: [] as string[] };
            if (roles instanceof Array) {

                u.roles.concat(roles);
            } else {
                u.roles.push(roles);
            }

            return <ILocalUser>u;

        }
        return <ILocalUser>{};
    }

    logout() {
        localStorage.removeItem('id_token');
        localStorage.removeItem('currentUser');
    }
}

