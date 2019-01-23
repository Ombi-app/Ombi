import { PlatformLocation } from "@angular/common";
import { Injectable } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class StatusService extends ServiceHelpers {
    constructor(http: HttpClient, public platformLocation: PlatformLocation) {
        super(http, "/api/v1/status/", platformLocation);
    }
    public getWizardStatus(): Observable<any> {
        return this.http.get(`${this.url}Wizard/`, {headers: this.headers});
    }
}
