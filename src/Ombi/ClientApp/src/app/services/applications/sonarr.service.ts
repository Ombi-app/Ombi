import { PlatformLocation, APP_BASE_HREF } from "@angular/common";
import { Injectable, Inject } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

import { ISonarrSettings, ITag } from "../../interfaces";
import { ILanguageProfiles, ISonarrProfile, ISonarrRootFolder } from "../../interfaces";
import { ServiceHelpers } from "../service.helpers";

@Injectable()
export class SonarrService extends ServiceHelpers {
    constructor(http: HttpClient, @Inject(APP_BASE_HREF) href:string) {
        super(http, "/api/v1/Sonarr", href);
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

    public getV3LanguageProfiles(settings: ISonarrSettings): Observable<ILanguageProfiles[]> {
        return this.http.post<ILanguageProfiles[]>(`${this.url}/v3/languageprofiles/`, JSON.stringify(settings), {headers: this.headers});
    }

    public getV3LanguageProfilesWithoutSettings(): Observable<ILanguageProfiles[]> {
        return this.http.get<ILanguageProfiles[]>(`${this.url}/v3/languageprofiles/`, {headers: this.headers});
    }

    public getTags(settings: ISonarrSettings): Observable<ITag[]> {
        return this.http.post<ITag[]>(`${this.url}/tags/`, JSON.stringify(settings), {headers: this.headers});
    }

    public isEnabled(): Observable<boolean> {
        return this.http.get<boolean>(`${this.url}/enabled/`, { headers: this.headers });
    }

    public getVersion(): Observable<string> {
        return this.http.get<string>(`${this.url}/version/`, { headers: this.headers });
    }
}
