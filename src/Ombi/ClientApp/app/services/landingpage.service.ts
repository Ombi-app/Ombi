import { Injectable } from "@angular/core";
import { Http } from "@angular/http";
import { Observable } from "rxjs/Rx";

import { IMediaServerStatus } from "../interfaces";
import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class LandingPageService extends ServiceHelpers {
    constructor(public http: Http) {
        super(http, "/api/v1/LandingPage/");
    }

    public getServerStatus(): Observable<IMediaServerStatus> {
        return this.http.get(`${this.url}`, { headers: this.headers }).map(this.extractData);
    }
}
