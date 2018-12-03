import { PlatformLocation } from "@angular/common";
import { Injectable } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

import { IMobileUsersViewModel } from "../interfaces";
import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class RequestRetryService extends ServiceHelpers {
    constructor(http: HttpClient, public platformLocation: PlatformLocation) {
        super(http, "/api/v1/requestretry/", platformLocation);
    }
    public getUserDeviceList(): Observable<IMobileUsersViewModel[]> {
        return this.http.get<IMobileUsersViewModel[]>(`${this.url}notification/`, {headers: this.headers});
    }
}
