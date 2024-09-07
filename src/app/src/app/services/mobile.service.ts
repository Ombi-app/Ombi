import { PlatformLocation, APP_BASE_HREF } from "@angular/common";
import { Injectable, Inject } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

import { IMobileUsersViewModel } from "../interfaces";
import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class MobileService extends ServiceHelpers {
    constructor(http: HttpClient, @Inject(APP_BASE_HREF) href:string) {
        super(http, "/api/v1/mobile/", href);
    }
    public getUserDeviceList(): Observable<IMobileUsersViewModel[]> {
        return this.http.get<IMobileUsersViewModel[]>(`${this.url}notification/`, {headers: this.headers});
    }

    public deleteUser(userId: string): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}`, { userId: userId }, {headers: this.headers});
    }
}
