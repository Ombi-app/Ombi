import { PlatformLocation } from "@angular/common";
import { Injectable } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

import { IMassEmailModel } from "../interfaces";

import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class NotificationMessageService extends ServiceHelpers {
    constructor(http: HttpClient, public platformLocation: PlatformLocation) {
        super(http, "/api/v1/notifications/", platformLocation);
    }
    public sendMassEmail(model: IMassEmailModel): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}massemail/`, JSON.stringify(model) ,{headers: this.headers});
    }
}
