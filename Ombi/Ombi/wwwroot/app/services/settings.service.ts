import { Injectable } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';
import { Observable } from 'rxjs/Rx';

import { ServiceAuthHelpers } from './service.helpers';
import { IOmbiSettings } from '../settings/interfaces/ISettings';


@Injectable()
export class SettingsService extends ServiceAuthHelpers {
    constructor(http: AuthHttp) {
        super(http, '/api/v1/Settings/');
    }
    
    getOmbi(): Observable<IOmbiSettings> {
        return this.http.get(this.url).map(this.extractData);
    }

    saveOmbi(settings: IOmbiSettings): Observable<boolean> {
        return this.http.post(`${this.url}/Ombi/`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }

}