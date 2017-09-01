import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Rx';
import { tokenNotExpired, JwtHelper } from 'angular2-jwt';
import { Http, Headers } from '@angular/http';

import { ServiceHelpers } from '../services/service.helpers';
import { IUserLogin, ILocalUser } from './IUserLogin';

@Injectable()
export class AuthService extends ServiceHelpers {
    constructor(http: Http) {
        super(http, '/api/v1/token');
    }

    jwtHelper: JwtHelper = new JwtHelper();

    login(login: IUserLogin): Observable<any> {
        this.headers = new Headers();
        this.headers.append('Content-Type', 'application/json');

        return this.http.post(`${this.url}/`, JSON.stringify(login), { headers: this.headers })
            .map(this.extractData);
    }

    loggedIn() {
        return tokenNotExpired('id_token');
    }

    claims(): ILocalUser {
        if (this.loggedIn()) {
            var token = localStorage.getItem('id_token');
            if (!token) {
                throw "Invalid token";
            }
            var json = this.jwtHelper.decodeToken(token);
            var roles = json["role"];
            var name = json["sub"];


            var u = { name: name, roles: [] as string[] };
            if (roles instanceof Array) {

                u.roles  = roles;
            } else {
                u.roles.push(roles);
            }
            return <ILocalUser>u;
        }
        return <ILocalUser>{};
    }


    hasRole(role: string): boolean {
        return this.claims().roles.some(r => r === role);
    }

    logout() {
        localStorage.removeItem('id_token');
    }
}

