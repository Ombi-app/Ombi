import { PlatformLocation } from "@angular/common";
import { Injectable } from "@angular/core";
import { Http } from "@angular/http";
import { AuthHttp } from "angular2-jwt";
import { Observable } from "rxjs/Rx";

import { ICheckbox, IIdentityResult, IResetPasswordToken, IUpdateLocalUser, IUser } from "../interfaces";
import { ServiceAuthHelpers } from "./service.helpers";

@Injectable()
export class IdentityService extends ServiceAuthHelpers {
    constructor(http: AuthHttp, private regularHttp: Http, public platformLocation: PlatformLocation) {
        super(http, "/api/v1/Identity/", platformLocation);
    }
    public createWizardUser(username: string, password: string): Observable<boolean> {
        return this.regularHttp.post(`${this.url}Wizard/`, JSON.stringify({ username, password }), { headers: this.headers }).map(this.extractData);
    }

    public getUser(): Observable<IUser> {
        return this.http.get(this.url).map(this.extractData);
    }

    public getUserById(id: string): Observable<IUser> {
        return this.http.get(`${this.url}User/${id}`).map(this.extractData);
    }

    public getUsers(): Observable<IUser[]> {
        return this.http.get(`${this.url}Users`).map(this.extractData);
    }

    public getAllAvailableClaims(): Observable<ICheckbox[]> {
        return this.http.get(`${this.url}Claims`).map(this.extractData);
    }

    public createUser(user: IUser): Observable<IIdentityResult> {
        return this.http.post(this.url, JSON.stringify(user), { headers: this.headers }).map(this.extractData);
    }

    public updateUser(user: IUser): Observable<IIdentityResult> {
        return this.http.put(this.url, JSON.stringify(user), { headers: this.headers }).map(this.extractData);
    }
    public updateLocalUser(user: IUpdateLocalUser): Observable<IIdentityResult> {
        return this.http.put(this.url + "local", JSON.stringify(user), { headers: this.headers }).map(this.extractData);
    }

    public deleteUser(user: IUser): Observable<IIdentityResult> {
        return this.http.delete(`${this.url}${user.id}`, { headers: this.headers }).map(this.extractData);
    }

    public submitResetPassword(email: string): Observable<IIdentityResult> {
        return this.regularHttp.post(this.url + "reset", JSON.stringify({email}), { headers: this.headers }).map(this.extractData);
    }

    public resetPassword(token: IResetPasswordToken): Observable<IIdentityResult> {
        return this.regularHttp.post(this.url + "resetpassword", JSON.stringify(token), { headers: this.headers }).map(this.extractData);
    }

    public sendWelcomeEmail(user: IUser): Observable<null> {
        return this.http.post(`${this.url}welcomeEmail`, JSON.stringify(user), { headers: this.headers }).map(this.extractData);
    }

    public hasRole(role: string): boolean {
        const roles = localStorage.getItem("roles") as string[] | null;
        if (roles) {
            if (roles.indexOf(role) > -1) {
                return true;
            }
            return false;
        }
        return false;
    }
}
