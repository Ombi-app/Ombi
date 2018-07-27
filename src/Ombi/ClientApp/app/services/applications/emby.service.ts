import { PlatformLocation } from "@angular/common";
import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";

import { ServiceHelpers } from "../service.helpers";

import { IEmbySettings, IUsersModel } from "../../interfaces";

@Injectable()
export class EmbyService extends ServiceHelpers {
    constructor(http: HttpClient, public platformLocation: PlatformLocation) {
        super(http, "/api/v1/Emby/", platformLocation);
    }

    public logIn(settings: IEmbySettings): Observable<IEmbySettings> {
        return this.http.post<IEmbySettings>(`${this.url}`, JSON.stringify(settings),  {headers: this.headers});
    }
    public getUsers(): Observable<IUsersModel[]> {
        return this.http.get<IUsersModel[]>(`${this.url}users`, {headers: this.headers});
    }

}
