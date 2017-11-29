import { PlatformLocation } from "@angular/common";
import { Injectable } from "@angular/core";
import { Http } from "@angular/http";
import { AuthHttp } from "angular2-jwt";
import { Observable } from "rxjs/Rx";

import { ICheckbox, ICreateWizardUser, IIdentityResult, IResetPasswordToken, IUpdateLocalUser, IUser } from "../interfaces";
import { ServiceAuthHelpers } from "./service.helpers";

@Injectable()
export class IdentityService extends ServiceAuthHelpers {
    constructor(http: AuthHttp, private regularHttp: Http, public platformLocation: PlatformLocation) {
        super(http, "/api/v1/Identity/", platformLocation);
    }
    public createWizardUser(user: ICreateWizardUser): Observable<boolean> {
        return this.regularHttp.post(`${this.url}Wizard/`, JSON.stringify(user), { headers: this.headers }).map(this.extractData).catch(this.handleError);
    }

    public getUser(): Observable<IUser> {
        return this.http.get(this.url).map(this.extractData).catch(this.handleError);
    }

    public getUserById(id: string): Observable<IUser> {
        return this.http.get(`${this.url}User/${id}`).map(this.extractData).catch(this.handleError);
    }

    public getUsers(): Observable<IUser[]> {
        return this.http.get(`${this.url}Users`).map(this.extractData).catch(this.handleError);
    }

    public getAllAvailableClaims(): Observable<ICheckbox[]> {
        return this.http.get(`${this.url}Claims`).map(this.extractData).catch(this.handleError);
    }

    public createUser(user: IUser): Observable<IIdentityResult> {
        return this.http.post(this.url, JSON.stringify(user), { headers: this.headers }).map(this.extractData).catch(this.handleError);
    }

    public updateUser(user: IUser): Observable<IIdentityResult> {
        return this.http.put(this.url, JSON.stringify(user), { headers: this.headers }).map(this.extractData).catch(this.handleError);
    }
    public updateLocalUser(user: IUpdateLocalUser): Observable<IIdentityResult> {
        return this.http.put(this.url + "local", JSON.stringify(user), { headers: this.headers }).map(this.extractData).catch(this.handleError);
    }

    public deleteUser(user: IUser): Observable<IIdentityResult> {
        return this.http.delete(`${this.url}${user.id}`, { headers: this.headers }).map(this.extractData).catch(this.handleError);
    }

    public hasUserRequested(userId: string): Observable<boolean> {
         return this.http.get(`${this.url}userhasrequest/${userId}`).map(this.extractData).catch(this.handleError);
    }

    public submitResetPassword(email: string): Observable<IIdentityResult> {
        return this.regularHttp.post(this.url + "reset", JSON.stringify({email}), { headers: this.headers }).map(this.extractData).catch(this.handleError);
    }

    public resetPassword(token: IResetPasswordToken): Observable<IIdentityResult> {
        return this.regularHttp.post(this.url + "resetpassword", JSON.stringify(token), { headers: this.headers }).map(this.extractData).catch(this.handleError);
    }

    public sendWelcomeEmail(user: IUser): Observable<null> {
        return this.http.post(`${this.url}welcomeEmail`, JSON.stringify(user), { headers: this.headers }).map(this.extractData).catch(this.handleError);
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
