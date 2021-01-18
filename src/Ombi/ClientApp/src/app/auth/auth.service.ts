import { PlatformLocation, APP_BASE_HREF } from "@angular/common";
import { HttpClient } from "@angular/common/http";
import { Injectable, Inject } from "@angular/core";
import { JwtHelperService } from "@auth0/angular-jwt";
import { Observable } from "rxjs";

import { ServiceHelpers } from "../services";
import { ILocalUser, IUserLogin } from "./IUserLogin";
import { StorageService } from "../shared/storage/storage-service";

@Injectable()
export class AuthService extends ServiceHelpers {

    constructor(http: HttpClient, @Inject(APP_BASE_HREF) href: string, private jwtHelperService: JwtHelperService,
                private store: StorageService) {
        super(http, "/api/v1/token", href);
    }

    public login(login: IUserLogin): Observable<any> {
        return this.http.post(`${this.url}/`, JSON.stringify(login), { headers: this.headers });
    }

    public oAuth(pin: number): Observable<any> {
        return this.http.get<any>(`${this.url}/${pin}`, { headers: this.headers });
    }

    public requiresPassword(login: IUserLogin): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}/requirePassword`, JSON.stringify(login), { headers: this.headers });
    }

    public getToken() {
        return this.jwtHelperService.tokenGetter();
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
            const token = this.store.get("id_token");
            if (!token) {
                throw new Error("Invalid token");
            }
            const json = this.jwtHelperService.decodeToken(token);
            const roles = json.role;
            const name = json.sub;
            const email = json.Email;

            const u = { name, roles: [] as string[], email };
            if (roles instanceof Array) {
                u.roles = roles;
            } else {
                u.roles.push(roles);
            }
            return <ILocalUser>u;
        }
        return <ILocalUser>{};
    }

    public hasRole(role: string): boolean {
        const claims = this.claims();

        if (claims && claims.roles && role && claims.roles.length > 0) {
            return claims.roles.some(r => r != undefined && r.toUpperCase() === role.toUpperCase());
        }
        return false;
    }

    public logout() {
        this.store.remove("id_token");
    }
}
