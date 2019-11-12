import { PlatformLocation } from "@angular/common";
import { HttpClient, HttpErrorResponse, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { empty, Observable, throwError } from "rxjs";
import { catchError } from "rxjs/operators";

import { IMovieDbKeyword } from "../../interfaces";
import { ServiceHelpers } from "../service.helpers";

@Injectable()
export class TheMovieDbService extends ServiceHelpers {
    constructor(http: HttpClient, public platformLocation: PlatformLocation) {
        super(http, "/api/v1/TheMovieDb", platformLocation);
    }

    public getKeywords(searchTerm: string): Observable<IMovieDbKeyword[]> {
        const params = new HttpParams().set("searchTerm", searchTerm);
        return this.http.get<IMovieDbKeyword[]>(`${this.url}/Keywords`, {headers: this.headers, params});
    }

    public getKeyword(keywordId: number): Observable<IMovieDbKeyword> {
        return this.http.get<IMovieDbKeyword>(`${this.url}/Keywords/${keywordId}`, { headers: this.headers })
            .pipe(catchError((error: HttpErrorResponse) => error.status === 404 ? empty() : throwError(error)));
    }
}
