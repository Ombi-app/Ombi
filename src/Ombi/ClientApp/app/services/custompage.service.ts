import { PlatformLocation } from "@angular/common";
import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";

import {
    ICustomPage,
} from "../interfaces";

import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class CustomPageService extends ServiceHelpers {
    constructor(public http: HttpClient, public platformLocation: PlatformLocation) {
        super(http, "/api/v1/CustomPage", platformLocation);
    } 

    public getCustomPage(): Observable<ICustomPage> {
        return this.http.get<ICustomPage>(this.url, {headers: this.headers});
    }

    public saveCustomPage(model: ICustomPage): Observable<boolean> {
        return this.http.post<boolean>(this.url, model, {headers: this.headers});
    }
}
