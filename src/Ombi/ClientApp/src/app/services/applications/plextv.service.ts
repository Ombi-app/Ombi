import { APP_BASE_HREF } from "@angular/common";
import { HttpClient } from "@angular/common/http";

import { IPlexPin } from "../../interfaces";
import { Injectable, Inject } from "@angular/core";
import { Observable } from "rxjs";
import { ServiceHelpers } from "../service.helpers";

@Injectable()
export class PlexTvService extends ServiceHelpers {

    constructor(http: HttpClient, @Inject(APP_BASE_HREF) href: string) {
        super(http, "/api/v1/token", href);
    }

    public GetPin(): Observable<IPlexPin> {
        // Create the Plex PIN through Ombi so the request is server-to-server. Creating a
        // strong PIN directly from the browser adds an Origin and can yield a PIN that Plex
        // authenticates in the popup but later refuses to redeem (1020).
        return this.http.post<IPlexPin>(`${this.url}/plexpin`, {}, { headers: this.headers });
    }
}
