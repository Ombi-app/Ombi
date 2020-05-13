import { APP_BASE_HREF } from "@angular/common";
import { Injectable, Inject } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

import { ICloudMobileDevices, ICloudMobileModel } from "../interfaces";
import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class CloudMobileService extends ServiceHelpers {
    constructor(http: HttpClient, @Inject(APP_BASE_HREF) href:string) {
        super(http, "/api/v2/mobile/", href);
    }
    public getDevices(): Observable<ICloudMobileModel[]> {
        return this.http.get<ICloudMobileModel[]>(`${this.url}users/`, {headers: this.headers});
    }

    public send(userId: string, message: string): Promise<boolean> {
        return this.http.post<boolean>(`${this.url}send/`, { userId, message }, {headers: this.headers}).toPromise();
    }
}
