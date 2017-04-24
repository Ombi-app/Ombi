import { Injectable } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';
import { Observable } from 'rxjs/Rx';

import { ServiceAuthHelpers } from './service.helpers';
import { IOmbiSettings, IEmbySettings, IPlexSettings, ISonarrSettings,ILandingPageSettings } from '../interfaces/ISettings';

@Injectable()
export class SettingsService extends ServiceAuthHelpers {
    constructor(http: AuthHttp) {
        super(http, '/api/v1/Settings/');
    }
    
    getOmbi(): Observable<IOmbiSettings> {
        return this.http.get(`${this.url}/Ombi/`).map(this.extractData);
    }

    saveOmbi(settings: IOmbiSettings): Observable<boolean> {
        return this.http.post(`${this.url}/Ombi/`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }

    getEmby(): Observable<IEmbySettings> {
        return this.http.get(`${this.url}/Emby/`).map(this.extractData);
    }

    saveEmby(settings: IEmbySettings): Observable<boolean> {
        return this.http.post(`${this.url}/Emby/`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }

    getPlex(): Observable<IPlexSettings> {
        return this.http.get(`${this.url}/Plex/`).map(this.extractData);
    }

    savePlex(settings: IPlexSettings): Observable<boolean> {
        return this.http.post(`${this.url}/Plex/`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }

    getSonarr(): Observable<ISonarrSettings> {
        return this.http.get(`${this.url}/Sonarr`).map(this.extractData);
    }

    saveSonarr(settings: ISonarrSettings): Observable<boolean> {
        return this.http.post(`${this.url}/Sonarr`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }
    getLandingPage(): Observable<ILandingPageSettings> {
        return this.http.get(`${this.url}/LandingPage`).map(this.extractData);
    }

    saveLandingPage(settings: ILandingPageSettings): Observable<boolean> {
        return this.http.post(`${this.url}/LandingPage`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }

}