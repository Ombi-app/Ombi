import { Injectable } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';
import { Observable } from 'rxjs/Rx';

import { ServiceAuthHelpers } from '../service.helpers';
import { ISonarrSettings } from '../../interfaces/ISettings';
import { ISonarrRootFolder, ISonarrProfile } from '../../interfaces/ISonarr';

@Injectable()
export class SonarrService extends ServiceAuthHelpers {
    constructor(http: AuthHttp) {
        super(http, '/api/v1/Sonarr/');
    }

    getRootFolders(settings: ISonarrSettings): Observable<ISonarrRootFolder[]> {
        return this.http.post(`${this.url}/RootFolders/`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }
    getQualityProfiles(settings: ISonarrSettings): Observable<ISonarrProfile[]> {
        return this.http.post(`${this.url}/Profiles/`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }
}