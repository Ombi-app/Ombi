import { PlatformLocation } from "@angular/common";
import { Injectable } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

import { IMobileUsersViewModel } from "../interfaces";
import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class MobileService extends ServiceHelpers {
    constructor(http: HttpClient, public platformLocation: PlatformLocation) {
        super(http, "/api/v1/mobile/", platformLocation);
    }
    public getUserDeviceList(): Observable<IMobileUsersViewModel[]> {
        return this.http.get<IMobileUsersViewModel[]>(`${this.url}notification/`, {headers: this.headers});
    }

    public deleteUser(userId: string): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}remove/`, userId, {headers: this.headers});
    }
}
