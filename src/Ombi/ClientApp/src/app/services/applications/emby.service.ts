import { PlatformLocation, APP_BASE_HREF } from "@angular/common";
import { HttpClient } from "@angular/common/http";
import { Injectable, Inject } from "@angular/core";
import { Observable } from "rxjs";

import { ServiceHelpers } from "../service.helpers";

import { IEmbyServer, IEmbySettings, IPublicInfo, IUsersModel } from "../../interfaces";

@Injectable()
export class EmbyService extends ServiceHelpers {
    constructor(http: HttpClient, @Inject(APP_BASE_HREF) href:string) {
        super(http, "/api/v1/Emby/", href);
    }

    public logIn(settings: IEmbySettings): Observable<IEmbySettings> {
        return this.http.post<IEmbySettings>(`${this.url}`, JSON.stringify(settings),  {headers: this.headers});
    }

    public getUsers(): Observable<IUsersModel[]> {
        return this.http.get<IUsersModel[]>(`${this.url}users`, {headers: this.headers});
    }
    
    public getPublicInfo(server: IEmbyServer): Observable<IPublicInfo> {
        return this.http.post<IPublicInfo>(`${this.url}info`, JSON.stringify(server), {headers: this.headers});
    }

}
