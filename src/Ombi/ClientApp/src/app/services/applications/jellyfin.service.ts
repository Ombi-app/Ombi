import { PlatformLocation, APP_BASE_HREF } from "@angular/common";
import { HttpClient } from "@angular/common/http";
import { Injectable, Inject } from "@angular/core";
import { Observable } from "rxjs";

import { ServiceHelpers } from "../service.helpers";

import { IJellyfinServer, IJellyfinSettings, IPublicInfo, IUsersModel } from "../../interfaces";

@Injectable()
export class JellyfinService extends ServiceHelpers {
    constructor(http: HttpClient, @Inject(APP_BASE_HREF) href:string) {
        super(http, "/api/v1/Jellyfin/", href);
    }

    public logIn(settings: IJellyfinSettings): Observable<IJellyfinSettings> {
        return this.http.post<IJellyfinSettings>(`${this.url}`, JSON.stringify(settings),  {headers: this.headers});
    }

    public getUsers(): Observable<IUsersModel[]> {
        return this.http.get<IUsersModel[]>(`${this.url}users`, {headers: this.headers});
    }
    
    public getPublicInfo(server: IJellyfinServer): Observable<IPublicInfo> {
        return this.http.post<IPublicInfo>(`${this.url}info`, JSON.stringify(server), {headers: this.headers});
    }

}
