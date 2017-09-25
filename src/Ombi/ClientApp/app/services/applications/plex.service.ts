import { Injectable } from "@angular/core";
import { Http } from "@angular/http";

import { AuthHttp } from "angular2-jwt";
import { Observable } from "rxjs/Rx";

import { ServiceAuthHelpers } from "../service.helpers";

import { IPlexAuthentication, IPlexLibResponse, IPlexServerViewModel } from "../../interfaces";
import { IPlexServer } from "../../interfaces";

@Injectable()
export class PlexService extends ServiceAuthHelpers {
    constructor(http: AuthHttp, private regularHttp: Http) {
        super(http, "/api/v1/Plex/");
    }

    public logIn(login: string, password: string): Observable<IPlexAuthentication> {
        return this.regularHttp.post(`${this.url}`, JSON.stringify({ login, password }), { headers: this.headers }).map(this.extractData);
    }

    public getServers(login: string, password: string): Observable<IPlexServerViewModel> {
        return this.http.post(`${this.url}servers`, JSON.stringify({ login, password }), { headers: this.headers }).map(this.extractData);
    }

    public getLibraries(plexSettings: IPlexServer): Observable<IPlexLibResponse> {
        return this.http.post(`${this.url}Libraries`, JSON.stringify(plexSettings), { headers: this.headers }).map(this.extractData).catch(this.handleError);
    }

}
