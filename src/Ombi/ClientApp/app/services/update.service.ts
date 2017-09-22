import { Injectable } from "@angular/core";
import { AuthHttp } from "angular2-jwt";
import { Observable } from "rxjs/Rx";

import { ServiceAuthHelpers } from "./service.helpers";

@Injectable()
export class UpdateService extends ServiceAuthHelpers {
    constructor(http: AuthHttp) {
        super(http, "/api/v1/Job/");
    }
    public forceUpdate(): Observable<boolean> {
        return this.http.post(`${this.url}update/`, { headers: this.headers }).map(this.extractData);
    }

    public checkForNewUpdate(): Observable<boolean> {
        return this.http.get(`${this.url}update/`).map(this.extractData);
    }
}
