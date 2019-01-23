import { PlatformLocation } from "@angular/common";
import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { JwtHelperService } from "@auth0/angular-jwt";
import { Observable } from "rxjs";

import { ServiceHelpers } from "../services";
import { ILocalUser, IUserLogin } from "./IUserLogin";

@Injectable()
export class AuthService extends ServiceHelpers {

    constructor(http: HttpClient, public platformLocation: PlatformLocation, private jwtHelperService: JwtHelperService) {
        super(http, "/api/v1/token", platformLocation);
    }

    public login(login: IUserLogin): Observable<any> {
        return this.http.post(`${this.url}/`, JSON.stringify(login), {headers: this.headers});
    }

    public oAuth(pin: number): Observable<any> {
        return this.http.get<any>(`${this.url}/${pin}`, {headers: this.headers});
    }

    public requiresPassword(login: IUserLogin): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}/requirePassword`, JSON.stringify(login), {headers: this.headers});
    }

    public loggedIn() {
        const token: string = this.jwtHelperService.tokenGetter();

        if (!token) {
            return false;
        }

        const tokenExpired: boolean = this.jwtHelperService.isTokenExpired(token);
        return !tokenExpired;
    }

    public claims(): ILocalUser {
        if (this.loggedIn()) {
            const token = localStorage.getItem("id_token");
            if (!token) {
                throw new Error("Invalid token");
            }
            const json = this.jwtHelperService.decodeToken(token);
            const roles = json.role;
            const name = json.sub;

            const u = { name, roles: [] as string[] };
            if (roles instanceof Array) {
                u.roles  = roles;
            } else {
                u.roles.push(roles);
            }
            return <ILocalUser> u;
        }
        return <ILocalUser> { };
    }

    public hasRole(role: string): boolean {
        return this.claims().roles.some(r => r.toUpperCase() === role.toUpperCase());
    }

    public logout() {
        localStorage.removeItem("id_token");
    }
}
