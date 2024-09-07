import { APP_BASE_HREF } from "@angular/common";
import { Injectable, Inject } from "@angular/core";
import { Observable } from "rxjs";

import { HttpClient } from "@angular/common/http";

import { IConnectedUser } from "../interfaces";
import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class HubService extends ServiceHelpers {
    constructor(public http: HttpClient, @Inject(APP_BASE_HREF) href:string) {
        super(http, "/api/v2/hub/", href);
    }

    public getConnectedUsers(): Promise<IConnectedUser[]> {
        return this.http.get<IConnectedUser[]>(`${this.url}users`, {headers: this.headers}).toPromise();
    }
}
