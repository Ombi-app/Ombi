import { PlatformLocation } from "@angular/common";
import { Headers, Http, Response } from "@angular/http";
import "rxjs/add/observable/throw";

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
        return body;
    }

    protected handleError(error: Response | any) {
        let errMsg: string;
        if (error instanceof Response) {
            const body = error.json() || "";
            const err = body.error || JSON.stringify(body);
            errMsg = `${error.status} - ${error.statusText || ""} ${err}`;
        } else {
            errMsg = error.message ? error.message : error.toString();
        }
        console.error(errMsg);
        return errMsg;
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
        if(res.text()) {
            const body = res.json();
            return body;
        } else {
            return "";
        }
    }

    protected extractContentData(res: Response) {
        if(res.text()) {
            return res.text();
        } else {
            return "";
        }
    }

    protected handleError(error: Response | any) {
        let errMsg: string;
        if (error instanceof Response) {
            const body = error.json() || "";
            const err = body.error || JSON.stringify(body);
            errMsg = `${error.status} - ${error.statusText || ""} ${err}`;
        } else {
            errMsg = error.Message ? error.message : error.toString();
        }
        console.error(errMsg);
        return errMsg;
    }
}
