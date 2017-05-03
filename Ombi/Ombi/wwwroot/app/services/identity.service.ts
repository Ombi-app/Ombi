import { Injectable } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';
import { Http } from '@angular/http';
import { Observable } from 'rxjs/Rx';

import { ServiceAuthHelpers } from './service.helpers';
import { IUser, ICheckbox } from '../interfaces/IUser';


@Injectable()
export class IdentityService extends ServiceAuthHelpers {
    constructor(http: AuthHttp, private regularHttp: Http) {
        super(http, '/api/v1/Identity/');
    }
    createWizardUser(username: string, password: string): Observable<boolean> {
        return this.regularHttp.post(`${this.url}/Wizard/`, JSON.stringify({ username: username, password: password }), { headers: this.headers }).map(this.extractData);
    }

    getUser(): Observable<IUser> {
        return this.http.get(this.url).map(this.extractData);
    }

    getUsers(): Observable<IUser[]> {
        return this.http.get(`${this.url}/Users`).map(this.extractData);
    }

    getAllAvailableClaims(): Observable<ICheckbox[]> {
        return this.http.get(`${this.url}/Claims`).map(this.extractData);
    }

    createUser(user: IUser): Observable<IUser> {
        return this.http.post(this.url, JSON.stringify(user), { headers: this.headers }).map(this.extractData);
    }

    updateUser(user: IUser): Observable<IUser> {
        return this.http.put(this.url, JSON.stringify(user), { headers: this.headers }).map(this.extractData);
    }

    hasRole(role: string): boolean {
        var roles = localStorage.getItem("roles") as string[];
        if (roles) {
            if (roles.indexOf(role) > -1) {
                return true;
            }
            return false;
        }
        return false;
    }
}