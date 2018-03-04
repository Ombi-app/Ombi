﻿import { PlatformLocation } from "@angular/common";
import { Injectable } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs/Rx";

import { ISonarrSettings } from "../../interfaces";
import { ISonarrProfile, ISonarrRootFolder } from "../../interfaces";
import { ServiceHelpers } from "../service.helpers";

@Injectable()
export class SonarrService extends ServiceHelpers {
    constructor(http: HttpClient, public platformLocation: PlatformLocation) {
        super(http, "/api/v1/Sonarr", platformLocation);
    }

    public getRootFolders(settings: ISonarrSettings): Observable<ISonarrRootFolder[]> {
        return this.http.post<ISonarrRootFolder[]>(`${this.url}/RootFolders/`, JSON.stringify(settings),  {headers: this.headers});
    }
    public getQualityProfiles(settings: ISonarrSettings): Observable<ISonarrProfile[]> {
        return this.http.post<ISonarrProfile[]>(`${this.url}/Profiles/`, JSON.stringify(settings),  {headers: this.headers});
    }

    public getRootFoldersWithoutSettings(): Observable<ISonarrRootFolder[]> {
        return this.http.get<ISonarrRootFolder[]>(`${this.url}/RootFolders/`,  {headers: this.headers});
    }
    public getQualityProfilesWithoutSettings(): Observable<ISonarrProfile[]> {
        return this.http.get<ISonarrProfile[]>(`${this.url}/Profiles/`, {headers: this.headers});
    }
}
