import { PlatformLocation, APP_BASE_HREF } from "@angular/common";
import { Injectable, Inject } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

import { ServiceHelpers } from "./service.helpers";
import { IUpdateModel } from "../interfaces";

@Injectable()
export class UpdateService extends ServiceHelpers {
    constructor(http: HttpClient, @Inject(APP_BASE_HREF) href:string) {
        super(http, "/api/v1/Update/", href);
    }
    public checkForUpdate(): Observable<IUpdateModel> {
        return this.http.get<IUpdateModel>(`${this.url}`, {headers: this.headers});
    }
}
