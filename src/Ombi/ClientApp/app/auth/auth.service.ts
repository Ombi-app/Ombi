import { Injectable } from "@angular/core";
import { Headers, Http } from "@angular/http";
import { JwtHelper, tokenNotExpired } from "angular2-jwt";
import { Observable } from "rxjs/Rx";

import { ServiceHelpers } from "../services";
import { ILocalUser, IUserLogin } from "./IUserLogin";

@Injectable()
export class AuthService extends ServiceHelpers {
    public jwtHelper: JwtHelper = new JwtHelper();

    constructor(http: Http) {
        super(http, "/api/v1/token");
    }

    public login(login: IUserLogin): Observable<any> {
        this.headers = new Headers();
        this.headers.append("Content-Type", "application/json");

        return this.http.post(`${this.url}/`, JSON.stringify(login), { headers: this.headers })
            .map(this.extractData);
    }

    public loggedIn() {
        return tokenNotExpired("id_token");
    }

    public claims(): ILocalUser {
        if (this.loggedIn()) {
            const token = localStorage.getItem("id_token");
            if (!token) {
                throw new Error("Invalid token");
            }
            const json = this.jwtHelper.decodeToken(token);
            const roles = json.role;
            const name = json.sub;

            const u = { name, roles: [] as string[] };
            if (roles instanceof Array) {

                u.roles  = roles;
            } else {
                u.roles.push(roles);
            }
            return <ILocalUser>u;
        }
        return <ILocalUser>{};
    }

    public hasRole(role: string): boolean {
        return this.claims().roles.some(r => r.toUpperCase() === role.toUpperCase());
    }

    public logout() {
        localStorage.removeItem("id_token");
    }
}
