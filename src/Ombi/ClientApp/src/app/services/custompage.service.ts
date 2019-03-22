import { PlatformLocation, APP_BASE_HREF } from "@angular/common";
import { HttpClient } from "@angular/common/http";
import { Injectable, Inject } from "@angular/core";
import { Observable } from "rxjs";

import {
    ICustomPage,
} from "../interfaces";

import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class CustomPageService extends ServiceHelpers {
    constructor(public http: HttpClient, @Inject(APP_BASE_HREF) href:string) {
        super(http, "/api/v1/CustomPage", href);
    } 

    public getCustomPage(): Observable<ICustomPage> {
        return this.http.get<ICustomPage>(this.url, {headers: this.headers});
    }

    public saveCustomPage(model: ICustomPage): Observable<boolean> {
        return this.http.post<boolean>(this.url, model, {headers: this.headers});
    }
}
