import { PlatformLocation, APP_BASE_HREF } from "@angular/common";
import { HttpClient } from "@angular/common/http";
import { Injectable, Inject } from "@angular/core";
import { Observable } from "rxjs";

import { ServiceHelpers } from "../service.helpers";

import { ICouchPotatoApiKey, ICouchPotatoProfiles, ICouchPotatoSettings } from "../../interfaces";

@Injectable()
export class CouchPotatoService extends ServiceHelpers {
    constructor(http: HttpClient, @Inject(APP_BASE_HREF) href:string) {
        super(http, "/api/v1/CouchPotato/", href);
    }

    public getProfiles(settings: ICouchPotatoSettings): Observable<ICouchPotatoProfiles> {
        return this.http.post<ICouchPotatoProfiles>(`${this.url}profile`, JSON.stringify(settings),  {headers: this.headers});
    }

    public getApiKey(settings: ICouchPotatoSettings): Observable<ICouchPotatoApiKey> {
        return this.http.post<ICouchPotatoApiKey>(`${this.url}apikey`, JSON.stringify(settings),  {headers: this.headers});
    }
}
