import { Injectable } from "@angular/core";
import { Http } from "@angular/http";
import { Observable } from "rxjs/Rx";

import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class StatusService extends ServiceHelpers {
    constructor(http: Http) {
        super(http, "/api/v1/status/");
    }
    public getWizardStatus(): Observable<any> {
        return this.http.get(`${this.url}Wizard/`, { headers: this.headers }).map(this.extractData);
    }
}
