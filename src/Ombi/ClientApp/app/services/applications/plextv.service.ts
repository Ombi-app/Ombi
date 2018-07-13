﻿import { PlatformLocation } from "@angular/common";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { Observable } from "rxjs/Rx";

import { IPlexPin } from "../../interfaces";

@Injectable()
export class PlexTvService {
    
    constructor(private http: HttpClient, public platformLocation: PlatformLocation) {
      
    }

    public GetPin(clientId: string, applicationName: string): Observable<IPlexPin> {
        const headers = new HttpHeaders({"Content-Type":"application/json",
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
