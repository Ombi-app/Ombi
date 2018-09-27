import { PlatformLocation } from "@angular/common";
import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";

import { ILidarrProfile, ILidarrRootFolder, IProfiles } from "../../interfaces";
import { ILidarrSettings } from "../../interfaces";
import { ServiceHelpers } from "../service.helpers";

@Injectable()
export class LidarrService extends ServiceHelpers {
    constructor(http: HttpClient, public platformLocation: PlatformLocation) {
        super(http, "/api/v1/Lidarr", platformLocation);
    }

    public getRootFolders(settings: ILidarrSettings): Observable<ILidarrRootFolder[]> {
        return this.http.post<ILidarrRootFolder[]>(`${this.url}/RootFolders/`, JSON.stringify(settings),  {headers: this.headers});
    }
    public getQualityProfiles(settings: ILidarrSettings): Observable<ILidarrProfile[]> {
        return this.http.post<ILidarrProfile[]>(`${this.url}/Profiles/`, JSON.stringify(settings),  {headers: this.headers});
    }

    public getRootFoldersFromSettings(): Observable<ILidarrRootFolder[]> {
        return this.http.get<ILidarrRootFolder[]>(`${this.url}/RootFolders/`,  {headers: this.headers});
    }
    public getQualityProfilesFromSettings(): Observable<ILidarrProfile[]> {
        return this.http.get<ILidarrProfile[]>(`${this.url}/Profiles/`,  {headers: this.headers});
    }

    public getMetadataProfiles(settings: ILidarrSettings): Observable<IProfiles[]> {
        return this.http.post<IProfiles[]>(`${this.url}/Metadata/`, JSON.stringify(settings), {headers: this.headers});
    }    
    public getLanguages(settings: ILidarrSettings): Observable<IProfiles[]> {
        return this.http.post<IProfiles[]>(`${this.url}/Langauges/`,JSON.stringify(settings), {headers: this.headers});
    }
}
