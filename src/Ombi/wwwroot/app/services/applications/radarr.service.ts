import { Injectable } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';
import { Observable } from 'rxjs/Rx';

import { ServiceAuthHelpers } from '../service.helpers';
import { IRadarrSettings } from '../../interfaces/ISettings';
import { IRadarrProfile, IRadarrRootFolder } from '../../interfaces/IRadarr';

@Injectable()
export class RadarrService extends ServiceAuthHelpers {
    constructor(http: AuthHttp) {
        super(http, '/api/v1/Radarr');
    }

    getRootFolders(settings: IRadarrSettings): Observable<IRadarrRootFolder[]> {
        return this.http.post(`${this.url}/RootFolders/`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }
    getQualityProfiles(settings: IRadarrSettings): Observable<IRadarrProfile[]> {
        return this.http.post(`${this.url}/Profiles/`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }
}