import { PlatformLocation, APP_BASE_HREF } from "@angular/common";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Injectable, Inject } from "@angular/core";

import { Observable } from "rxjs";

import { IPlexPin } from "../../interfaces";

@Injectable()
export class PlexTvService {

    constructor(private http: HttpClient) {
    }

    public GetPin(clientId: string, applicationName: string): Observable<IPlexPin> {
        const headers = new HttpHeaders({"Content-Type": "application/json",
        "X-Plex-Client-Identifier": clientId,
        "X-Plex-Product": applicationName,
        "X-Plex-Version": "3",
        "X-Plex-Device": "Ombi (Web)",
        "X-Plex-Platform": "Web",
        "Accept": "application/json",
    });
        return this.http.post<IPlexPin>("https://plex.tv/api/v2/pins?strong=true", null,  {headers});
    }

}
