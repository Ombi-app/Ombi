import { PlatformLocation } from "@angular/common";
import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { Observable } from "rxjs";

import { ServiceHelpers } from "../service.helpers";

import { IPlexOAuthAccessToken } from "../../interfaces";

@Injectable()
export class PlexOAuthService extends ServiceHelpers {
    constructor(http: HttpClient, public platformLocation: PlatformLocation) {
        super(http, "/api/v1/PlexOAuth/", platformLocation);
    }

    public oAuth(pin: number): Observable<IPlexOAuthAccessToken> {
        return this.http.get<IPlexOAuthAccessToken>(`${this.url}${pin}`,  {headers: this.headers});
    }
}
