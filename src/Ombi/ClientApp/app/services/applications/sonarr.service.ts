import { Injectable } from "@angular/core";
import { AuthHttp } from "angular2-jwt";
import { Observable } from "rxjs/Rx";

import { ISonarrSettings } from "../../interfaces";
import { ISonarrProfile, ISonarrRootFolder } from "../../interfaces";
import { ServiceAuthHelpers } from "../service.helpers";

@Injectable()
export class SonarrService extends ServiceAuthHelpers {
    constructor(http: AuthHttp) {
        super(http, "/api/v1/Sonarr");
    }

    public getRootFolders(settings: ISonarrSettings): Observable<ISonarrRootFolder[]> {
        return this.http.post(`${this.url}/RootFolders/`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }
    public getQualityProfiles(settings: ISonarrSettings): Observable<ISonarrProfile[]> {
        return this.http.post(`${this.url}/Profiles/`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }
}
