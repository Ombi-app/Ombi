import { PlatformLocation } from "@angular/common";
import { Injectable } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

import { ICheckbox, ICreateWizardUser, IIdentityResult, INotificationPreferences, IResetPasswordToken, IUpdateLocalUser, IUser, IWizardUserResult } from "../interfaces";
import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class IdentityService extends ServiceHelpers {
    constructor(http: HttpClient, public platformLocation: PlatformLocation) {
        super(http, "/api/v1/Identity/", platformLocation);
    }
    public createWizardUser(user: ICreateWizardUser): Observable<IWizardUserResult> {
        return this.http.post<IWizardUserResult>(`${this.url}Wizard/`, JSON.stringify(user),  {headers: this.headers});
    }

    public getUser(): Observable<IUser> {
        return this.http.get<IUser>(this.url,  {headers: this.headers});
    }

    public getAccessToken(): Observable<string> {
        return this.http.get<string>(`${this.url}accesstoken`,  {headers: this.headers});
    }

    public getUserById(id: string): Observable<IUser> {
        return this.http.get<IUser>(`${this.url}User/${id}`,  {headers: this.headers});
    }

    public getUsers(): Observable<IUser[]> {
        return this.http.get<IUser[]>(`${this.url}Users`,  {headers: this.headers});
    }

    public getAllAvailableClaims(): Observable<ICheckbox[]> {
        return this.http.get<ICheckbox[]>(`${this.url}Claims`,  {headers: this.headers});
    }

    public createUser(user: IUser): Observable<IIdentityResult> {
        return this.http.post<IIdentityResult>(this.url, JSON.stringify(user), {headers: this.headers});
    }

    public updateUser(user: IUser): Observable<IIdentityResult> {
        return this.http.put<IIdentityResult>(this.url, JSON.stringify(user), {headers: this.headers});
    }

    public updateNotificationPreferences(pref: INotificationPreferences[]): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}NotificationPreferences`, JSON.stringify(pref), {headers: this.headers});
    }

    public updateLocalUser(user: IUpdateLocalUser): Observable<IIdentityResult> {
        return this.http.put<IIdentityResult>(this.url + "local", JSON.stringify(user), {headers: this.headers});
    }

    public deleteUser(user: IUser): Observable<IIdentityResult> {
        return this.http.delete<IIdentityResult>(`${this.url}${user.id}`, {headers: this.headers});
    }

    public hasUserRequested(userId: string): Observable<boolean> {
         return this.http.get<boolean>(`${this.url}userhasrequest/${userId}`, {headers: this.headers});
    }

    public submitResetPassword(email: string): Observable<IIdentityResult> {
        return this.http.post<IIdentityResult>(this.url + "reset", JSON.stringify({email}), {headers: this.headers});
    }

    public resetPassword(token: IResetPasswordToken): Observable<IIdentityResult> {
        return this.http.post<IIdentityResult>(this.url + "resetpassword", JSON.stringify(token), {headers: this.headers});
    }

    public sendWelcomeEmail(user: IUser): Observable<null> {
        return this.http.post<any>(`${this.url}welcomeEmail`, JSON.stringify(user), {headers: this.headers});
    }

    public getNotificationPreferences(): Observable<INotificationPreferences[]> {
        return this.http.get<INotificationPreferences[]>(`${this.url}notificationpreferences`, {headers: this.headers});
    }
    public getNotificationPreferencesForUser(userId: string): Observable<INotificationPreferences[]> {
        return this.http.get<INotificationPreferences[]>(`${this.url}notificationpreferences/${userId}`, {headers: this.headers});
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
