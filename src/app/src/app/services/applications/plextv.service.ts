import { HttpClient, HttpHeaders } from "@angular/common/http";

import { IPlexPin } from "../../interfaces";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";

@Injectable()
export class PlexTvService {

    constructor(private http: HttpClient) {
    }

    public GetPin(clientId: string, applicationName: string): Observable<IPlexPin> {
        const headers = new HttpHeaders({"Content-Type": "application/json; charset=ISO-8859-1",
        "X-Plex-Client-Identifier": clientId,
        "X-Plex-Product": applicationName,
        "X-Plex-Version": "3",
        "X-Plex-Device": "Ombi (Web)",
        "X-Plex-Platform": "Web",
        "Accept": "application/json",
        'X-Plex-Model': 'Plex OAuth',
    });
        return this.http.post<IPlexPin>("https://plex.tv/api/v2/pins?strong=true", null,  {headers});
    }

}
