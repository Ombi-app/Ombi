import { PlatformLocation } from "@angular/common";
import { Headers, Http, Response } from "@angular/http";
import "rxjs/add/observable/throw";
import { Observable } from "rxjs/Observable";

import { AuthHttp } from "angular2-jwt";

export class ServiceHelpers {

    protected headers: Headers;

    constructor(protected http: Http, protected url: string, protected platformLocation: PlatformLocation) {
        const base = platformLocation.getBaseHrefFromDOM();
        if (base.length > 1) {
            this.url = base + this.url;
        }
        this.headers = new Headers();
        this.headers.append("Content-Type", "application/json; charset=utf-8");
    }

    protected extractData(res: Response) {
        const body = res.json();
        //console.log('extractData', body || {});
        return body || {};
    }

    protected handleError(error: any) {
        // In a real world app, we might use a remote logging infrastructure
        // We'd also dig deeper into the error to get a better message
        const errMsg = (error.message) ? error.message :
            error.status ? `${error.status} - ${error.statusText}` : "Server error";
        return Observable.throw(errMsg);
    }
}

export class ServiceAuthHelpers {

    protected headers: Headers;

    constructor(protected http: AuthHttp, protected url: string, protected platformLocation: PlatformLocation) {
        const base = platformLocation.getBaseHrefFromDOM();
        if (base.length > 1) {
            this.url = base + this.url;
        }
        this.headers = new Headers();
        this.headers.append("Content-Type", "application/json; charset=utf-8");
    }

    protected extractData(res: Response) {
        const body = res.json();
        //console.log('extractData', body || {});
        return body || {};
    }

    protected handleError(error: any) {
        // In a real world app, we might use a remote logging infrastructure
        // We'd also dig deeper into the error to get a better message
        const errMsg = (error.message) ? error.message :
            error.status ? `${error.status} - ${error.statusText}` : "Server error";
        return Observable.throw(errMsg);
    }
}
