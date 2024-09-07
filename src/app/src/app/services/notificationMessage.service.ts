import { PlatformLocation, APP_BASE_HREF } from "@angular/common";
import { Injectable, Inject } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

import { IMassEmailModel } from "../interfaces";

import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class NotificationMessageService extends ServiceHelpers {
    constructor(http: HttpClient, @Inject(APP_BASE_HREF) href:string) {
        super(http, "/api/v1/notifications/", href);
    }
    public sendMassEmail(model: IMassEmailModel): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}massemail/`, JSON.stringify(model) ,{headers: this.headers});
    }
}
