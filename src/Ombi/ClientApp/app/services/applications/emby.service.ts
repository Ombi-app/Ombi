import { Injectable } from "@angular/core";
import {  Http } from "@angular/http";
import { Observable } from "rxjs/Rx";

import { ServiceHelpers } from "../service.helpers";

import { IEmbySettings } from "../../interfaces";

@Injectable()
export class EmbyService extends ServiceHelpers {
    constructor(http: Http) {
        super(http, "/api/v1/Emby/");
    }

    public logIn(settings: IEmbySettings): Observable<IEmbySettings> {
        return this.http.post(`${this.url}`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }

}
