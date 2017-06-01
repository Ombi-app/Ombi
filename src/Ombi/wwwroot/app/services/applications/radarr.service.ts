import { Injectable } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';
//import { Observable } from 'rxjs/Rx';

import { ServiceAuthHelpers } from '../service.helpers';
//import { IRadarrSettings } from '../../interfaces/ISettings';

@Injectable()
export class RadarrService extends ServiceAuthHelpers {
    constructor(http: AuthHttp) {
        super(http, '/api/v1/Radarr');
    }

    // getRootFolders(settings: IRadarrSettings): Observable<ISonarrRootFolder[]> {
    //     return this.http.post(`${this.url}/RootFolders/`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    // }
    // getQualityProfiles(settings: IRadarrSettings): Observable<ISonarrProfile[]> {
    //     return this.http.post(`${this.url}/Profiles/`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    // }
}