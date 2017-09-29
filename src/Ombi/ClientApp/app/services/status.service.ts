import { Injectable } from "@angular/core";
import { PlatformLocation } from "@angular/common";
import { Http } from "@angular/http";
import { Observable } from "rxjs/Rx";

import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class StatusService extends ServiceHelpers {
    constructor(http: Http, public platformLocation: PlatformLocation) {
        super(http, "/api/v1/status/", platformLocation);
    }
    public getWizardStatus(): Observable<any> {
        return this.http.get(`${this.url}Wizard/`, { headers: this.headers }).map(this.extractData);
    }
}
