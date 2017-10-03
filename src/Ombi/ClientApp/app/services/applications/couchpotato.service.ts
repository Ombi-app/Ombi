import { PlatformLocation } from "@angular/common";
import { Injectable } from "@angular/core";
import { AuthHttp } from "angular2-jwt";
import { Observable } from "rxjs/Rx";

import { ServiceAuthHelpers } from "../service.helpers";

import { ICouchPotatoApiKey, ICouchPotatoProfiles, ICouchPotatoSettings } from "../../interfaces";

@Injectable()
export class CouchPotatoService extends ServiceAuthHelpers {
    constructor(http: AuthHttp, public platformLocation: PlatformLocation) {
        super(http, "/api/v1/CouchPotato/", platformLocation);
    }

    public getProfiles(settings: ICouchPotatoSettings): Observable<ICouchPotatoProfiles> {
        return this.http.post(`${this.url}profile`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }

    public getApiKey(settings: ICouchPotatoSettings): Observable<ICouchPotatoApiKey> {
        return this.http.post(`${this.url}apikey`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }
}
