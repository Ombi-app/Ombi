import { PlatformLocation, APP_BASE_HREF } from "@angular/common";
import { HttpClient } from "@angular/common/http";
import { Injectable, Inject } from "@angular/core";
import { Observable } from "rxjs";

import { IRadarrProfile, IRadarrRootFolder } from "../../interfaces";
import { IRadarrSettings } from "../../interfaces";
import { ServiceHelpers } from "../service.helpers";

@Injectable()
export class RadarrService extends ServiceHelpers {
    constructor(http: HttpClient, @Inject(APP_BASE_HREF) href:string) {
        super(http, "/api/v1/Radarr", href);
    }

    public getRootFolders(settings: IRadarrSettings): Observable<IRadarrRootFolder[]> {
        return this.http.post<IRadarrRootFolder[]>(`${this.url}/RootFolders/`, JSON.stringify(settings), { headers: this.headers });
    }
    public getQualityProfiles(settings: IRadarrSettings): Observable<IRadarrProfile[]> {
        return this.http.post<IRadarrProfile[]>(`${this.url}/Profiles/`, JSON.stringify(settings), { headers: this.headers });
    }

    public getRootFoldersFromSettings(): Observable<IRadarrRootFolder[]> {
        return this.http.get<IRadarrRootFolder[]>(`${this.url}/RootFolders/`, { headers: this.headers });
    }
    public getQualityProfilesFromSettings(): Observable<IRadarrProfile[]> {
        return this.http.get<IRadarrProfile[]>(`${this.url}/Profiles/`, { headers: this.headers });
    }
    public isRadarrEnabled(): Promise<boolean> {
        return this.http.get<boolean>(`${this.url}/enabled/`, { headers: this.headers }).toPromise();
    }
}
