import { Headers, Response, Http } from '@angular/http';
import { Observable } from 'rxjs/Observable';


import { AuthHttp } from 'angular2-jwt';

export class ServiceHelpers {

    constructor(protected http: Http, protected url: string) {
        this.headers = new Headers();
        this.headers.append('Content-Type', 'application/json; charset=utf-8');
    }

    protected headers: Headers;

    protected extractData(res: Response) {
        let body = res.json();
        //console.log('extractData', body || {});
        return body || {};
    }

    protected handleError(error: any) {
        // In a real world app, we might use a remote logging infrastructure
        // We'd also dig deeper into the error to get a better message
        let errMsg = (error.message) ? error.message :
            error.status ? `${error.status} - ${error.statusText}` : 'Server error';
        return Observable.throw(errMsg);
    }


}

export class ServiceAuthHelpers {

    constructor(protected http: AuthHttp, protected url: string) {
        this.headers = new Headers();
        this.headers.append('Content-Type', 'application/json; charset=utf-8');
    }

    protected headers: Headers;

    protected extractData(res: Response) {
        let body = res.json();
        //console.log('extractData', body || {});
        return body || {};
    }

    protected handleError(error: any) {
        // In a real world app, we might use a remote logging infrastructure
        // We'd also dig deeper into the error to get a better message
        let errMsg = (error.message) ? error.message :
            error.status ? `${error.status} - ${error.statusText}` : 'Server error';
        return Observable.throw(errMsg);
    }


}