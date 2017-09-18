import { Injectable } from "@angular/core";
import { Http } from "@angular/http";
import { Observable } from "rxjs/Rx";

import { IImages } from "../interfaces";
import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class ImageService extends ServiceHelpers {
    constructor(public http: Http) {
        super(http, "/api/v1/Images/");
    }

    public getRandomBackground(): Observable<IImages> {
        return this.http.get(`${this.url}background/`, { headers: this.headers }).map(this.extractData);
    }
}
