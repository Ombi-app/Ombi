import { PlatformLocation } from "@angular/common";
import { Injectable } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

import { IMultiSearchResult } from "../interfaces";
import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class SearchV2Service extends ServiceHelpers {
    constructor(http: HttpClient, public platformLocation: PlatformLocation) {
        super(http, "/api/v2/search", platformLocation);
    }

    public multiSearch(searchTerm: string): Observable<IMultiSearchResult[]> {
        return this.http.get<IMultiSearchResult[]>(`${this.url}/multi/${searchTerm}`);
    }
}
