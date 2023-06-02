import { PlatformLocation, APP_BASE_HREF } from "@angular/common";
import { Injectable, Inject } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class StatusService extends ServiceHelpers {
    constructor(http: HttpClient, @Inject(APP_BASE_HREF) href:string) {
        super(http, "/api/v1/status/", href);
    }
    public getWizardStatus(): Observable<WizardResult> {
        return this.http.get<WizardResult>(`${this.url}Wizard/`, {headers: this.headers});
    }
}

export interface WizardResult {
    result: boolean;
}
