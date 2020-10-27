
import { APP_BASE_HREF } from "@angular/common";
import { Injectable, Inject } from "@angular/core";

import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Observable } from "rxjs";

import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class SystemService extends ServiceHelpers {
    constructor(http: HttpClient, @Inject(APP_BASE_HREF) href:string) {
        super(http, "/api/v2/system/", href);
    }
    public getAvailableLogs(): Observable<string[]> {
        return this.http.get<string[]>(`${this.url}logs/`, {headers: this.headers});
    }
    public getLog(logName: string): Observable<string> {
        return this.http.get(`${this.url}logs/${logName}`, {responseType: 'text'});
    }
    public getNews(): Observable<string> {
        return this.http.get(`${this.url}news`, {responseType: 'text'});
    }
}
