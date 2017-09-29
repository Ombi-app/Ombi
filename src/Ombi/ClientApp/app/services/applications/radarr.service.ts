import { Injectable } from "@angular/core";
import { PlatformLocation } from "@angular/common";
import { AuthHttp } from "angular2-jwt";
import { Observable } from "rxjs/Rx";

import { IRadarrProfile, IRadarrRootFolder } from "../../interfaces";
import { IRadarrSettings } from "../../interfaces";
import { ServiceAuthHelpers } from "../service.helpers";

@Injectable()
export class RadarrService extends ServiceAuthHelpers {
    constructor(http: AuthHttp, public platformLocation: PlatformLocation) {
        super(http, "/api/v1/Radarr", platformLocation);
    }

    public getRootFolders(settings: IRadarrSettings): Observable<IRadarrRootFolder[]> {
        return this.http.post(`${this.url}/RootFolders/`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }
    public getQualityProfiles(settings: IRadarrSettings): Observable<IRadarrProfile[]> {
        return this.http.post(`${this.url}/Profiles/`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }
}
