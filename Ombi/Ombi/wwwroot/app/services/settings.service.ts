import { Injectable } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';
import { Http } from '@angular/http';
import { Observable } from 'rxjs/Rx';

import { ServiceAuthHelpers } from './service.helpers';
import { IOmbiSettings, IEmbySettings, IPlexSettings, ISonarrSettings,ILandingPageSettings, ICustomizationSettings } from '../interfaces/ISettings';

@Injectable()
export class SettingsService extends ServiceAuthHelpers {
    constructor(public httpAuth: AuthHttp, private nonAuthHttp: Http) {
        super(httpAuth, '/api/v1/Settings');
    }
    
    getOmbi(): Observable<IOmbiSettings> {
        return this.httpAuth.get(`${this.url}/Ombi/`).map(this.extractData).catch(this.handleError)
    }

    saveOmbi(settings: IOmbiSettings): Observable<boolean> {
        return this.httpAuth.post(`${this.url}/Ombi/`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData).catch(this.handleError)
    }

    getEmby(): Observable<IEmbySettings> {
        return this.httpAuth.get(`${this.url}/Emby/`).map(this.extractData).catch(this.handleError)
    }

    saveEmby(settings: IEmbySettings): Observable<boolean> {
        return this.httpAuth.post(`${this.url}/Emby/`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData).catch(this.handleError)
    }

    getPlex(): Observable<IPlexSettings> {
        return this.httpAuth.get(`${this.url}/Plex/`).map(this.extractData).catch(this.handleError)
    }

    savePlex(settings: IPlexSettings): Observable<boolean> {
        return this.httpAuth.post(`${this.url}/Plex/`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData).catch(this.handleError)
    }

    getSonarr(): Observable<ISonarrSettings> {
        return this.httpAuth.get(`${this.url}/Sonarr`).map(this.extractData)
            .catch(this.handleError);
    }

    saveSonarr(settings: ISonarrSettings): Observable<boolean> {
        return this.httpAuth.post(`${this.url}/Sonarr`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData).catch(this.handleError)
    }

    // Using http since we need it not to be authenticated to get the landing page settings
    getLandingPage(): Observable<ILandingPageSettings> {
        return this.nonAuthHttp.get(`${this.url}/LandingPage`).map(this.extractData).catch(this.handleError)
    }

    saveLandingPage(settings: ILandingPageSettings): Observable<boolean> {
        return this.httpAuth.post(`${this.url}/LandingPage`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData).catch(this.handleError)
    }

    // Using http since we need it not to be authenticated to get the customization settings
    getCustomization(): Observable<ICustomizationSettings> {
        return this.nonAuthHttp.get(`${this.url}/customization`).map(this.extractData).catch(this.handleError)
    }

    saveCustomization(settings: ICustomizationSettings): Observable<boolean> {
        return this.httpAuth.post(`${this.url}/customization`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData).catch(this.handleError)
    }



}