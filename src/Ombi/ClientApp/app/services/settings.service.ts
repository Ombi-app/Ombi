import { PlatformLocation } from "@angular/common";
import { Injectable } from "@angular/core";
import { Http } from "@angular/http";
import { AuthHttp } from "angular2-jwt";
import { Observable } from "rxjs/Rx";

import {
    IAbout,
    IAuthenticationSettings,
    ICustomizationSettings,
    IDiscordNotifcationSettings,
    IEmailNotificationSettings,
    IEmbySettings,
    ILandingPageSettings,
    IMattermostNotifcationSettings,
    IOmbiSettings,
    IPlexSettings,
    IPushbulletNotificationSettings,
    IPushoverNotificationSettings,
    IRadarrSettings,
    ISlackNotificationSettings,
    ISonarrSettings,
    IUpdateSettings,
    IUserManagementSettings,
} from "../interfaces";

import { ServiceAuthHelpers } from "./service.helpers";

@Injectable()
export class SettingsService extends ServiceAuthHelpers {
    constructor(public httpAuth: AuthHttp, private nonAuthHttp: Http,
                public platformLocation: PlatformLocation) {
        super(httpAuth, "/api/v1/Settings", platformLocation);
    }

    public about(): Observable<IAbout> {
        return this.httpAuth.get(`${this.url}/About/`).map(this.extractData).catch(this.handleError);
    }

    public getOmbi(): Observable<IOmbiSettings> {
        return this.httpAuth.get(`${this.url}/Ombi/`).map(this.extractData).catch(this.handleError);
    }

    public saveOmbi(settings: IOmbiSettings): Observable<boolean> {
        return this.httpAuth.post(`${this.url}/Ombi/`, JSON.stringify(settings), { headers: this.headers })
            .map(this.extractData).catch(this.handleError);
    }

    public resetOmbiApi(): Observable<string> {
        return this.httpAuth.post(`${this.url}/Ombi/resetApi`, { headers: this.headers }).map(this.extractData)
            .catch(this.handleError);
    }

    public getEmby(): Observable<IEmbySettings> {
        return this.httpAuth.get(`${this.url}/Emby/`).map(this.extractData).catch(this.handleError);
    }

    public saveEmby(settings: IEmbySettings): Observable<boolean> {
        return this.httpAuth.post(`${this.url}/Emby/`, JSON.stringify(settings), { headers: this.headers })
            .map(this.extractData).catch(this.handleError);
    }

    public getPlex(): Observable<IPlexSettings> {
        return this.httpAuth.get(`${this.url}/Plex/`).map(this.extractData).catch(this.handleError);
    }

    public savePlex(settings: IPlexSettings): Observable<boolean> {
        return this.httpAuth.post(`${this.url}/Plex/`, JSON.stringify(settings), { headers: this.headers })
            .map(this.extractData).catch(this.handleError);
    }

    public getSonarr(): Observable<ISonarrSettings> {
        return this.httpAuth.get(`${this.url}/Sonarr`).map(this.extractData)
            .catch(this.handleError);
    }

    public saveSonarr(settings: ISonarrSettings): Observable<boolean> {
        return this.httpAuth.post(`${this.url}/Sonarr`, JSON.stringify(settings), { headers: this.headers })
            .map(this.extractData).catch(this.handleError);
    }

    public getRadarr(): Observable<IRadarrSettings> {
        return this.httpAuth.get(`${this.url}/Radarr`).map(this.extractData)
            .catch(this.handleError);
    }

    public saveRadarr(settings: IRadarrSettings): Observable<boolean> {
        return this.httpAuth.post(`${this.url}/Radarr`, JSON.stringify(settings), { headers: this.headers })
            .map(this.extractData).catch(this.handleError);
    }

    public getAuthentication(): Observable<IAuthenticationSettings> {
        return this.httpAuth.get(`${this.url}/Authentication`).map(this.extractData)
            .catch(this.handleError);
    }

    public saveAuthentication(settings: IAuthenticationSettings): Observable<boolean> {
        return this.httpAuth.post(`${this.url}/Authentication`, JSON.stringify(settings), { headers: this.headers })
            .map(this.extractData).catch(this.handleError);
    }

    // Using http since we need it not to be authenticated to get the landing page settings
    public getLandingPage(): Observable<ILandingPageSettings> {
        return this.nonAuthHttp.get(`${this.url}/LandingPage`).map(this.extractData).catch(this.handleError);
    }

    public saveLandingPage(settings: ILandingPageSettings): Observable<boolean> {
        return this.httpAuth.post(`${this.url}/LandingPage`, JSON.stringify(settings), { headers: this.headers })
            .map(this.extractData).catch(this.handleError);
    }

    // Using http since we need it not to be authenticated to get the customization settings
    public getCustomization(): Observable<ICustomizationSettings> {
        return this.nonAuthHttp.get(`${this.url}/customization`).map(this.extractData).catch(this.handleError);
    }

    public saveCustomization(settings: ICustomizationSettings): Observable<boolean> {
        return this.httpAuth.post(`${this.url}/customization`, JSON.stringify(settings), { headers: this.headers })
            .map(this.extractData).catch(this.handleError);
    }

    public getEmailNotificationSettings(): Observable<IEmailNotificationSettings> {
        return this.httpAuth.get(`${this.url}/notifications/email`).map(this.extractData).catch(this.handleError);
    }

    public saveEmailNotificationSettings(settings: IEmailNotificationSettings): Observable<boolean> {
        return this.httpAuth
            .post(`${this.url}/notifications/email`, JSON.stringify(settings), { headers: this.headers })
            .map(this.extractData).catch(this.handleError);
    }

    public getDiscordNotificationSettings(): Observable<IDiscordNotifcationSettings> {
        return this.httpAuth.get(`${this.url}/notifications/discord`).map(this.extractData).catch(this.handleError);
    }

    public getMattermostNotificationSettings(): Observable<IMattermostNotifcationSettings> {
        return this.httpAuth.get(`${this.url}/notifications/mattermost`).map(this.extractData).catch(this.handleError);
    }

    public saveDiscordNotificationSettings(settings: IDiscordNotifcationSettings): Observable<boolean> {
        return this.httpAuth
            .post(`${this.url}/notifications/discord`, JSON.stringify(settings), { headers: this.headers })
            .map(this.extractData).catch(this.handleError);
    }

    public saveMattermostNotificationSettings(settings: IMattermostNotifcationSettings): Observable<boolean> {
        return this.httpAuth
            .post(`${this.url}/notifications/mattermost`, JSON.stringify(settings), { headers: this.headers })
            .map(this.extractData).catch(this.handleError);
    }
    public getPushbulletNotificationSettings(): Observable<IPushbulletNotificationSettings> {
        return this.httpAuth.get(`${this.url}/notifications/pushbullet`).map(this.extractData).catch(this.handleError);
    }
    public getPushoverNotificationSettings(): Observable<IPushoverNotificationSettings> {
        return this.httpAuth.get(`${this.url}/notifications/pushover`).map(this.extractData).catch(this.handleError);
    }

    public savePushbulletNotificationSettings(settings: IPushbulletNotificationSettings): Observable<boolean> {
        return this.httpAuth
            .post(`${this.url}/notifications/pushbullet`, JSON.stringify(settings), { headers: this.headers })
            .map(this.extractData).catch(this.handleError);
    }
    public savePushoverNotificationSettings(settings: IPushoverNotificationSettings): Observable<boolean> {
        return this.httpAuth
            .post(`${this.url}/notifications/pushover`, JSON.stringify(settings), { headers: this.headers })
            .map(this.extractData).catch(this.handleError);
    }

    public getSlackNotificationSettings(): Observable<ISlackNotificationSettings> {
        return this.httpAuth.get(`${this.url}/notifications/slack`).map(this.extractData).catch(this.handleError);
    }

    public saveSlackNotificationSettings(settings: ISlackNotificationSettings): Observable<boolean> {
        return this.httpAuth
            .post(`${this.url}/notifications/slack`, JSON.stringify(settings), { headers: this.headers })
            .map(this.extractData).catch(this.handleError);
    }

    public getUpdateSettings(): Observable<IUpdateSettings> {
        return this.httpAuth.get(`${this.url}/update`).map(this.extractData).catch(this.handleError);
    }

    public saveUpdateSettings(settings: IUpdateSettings): Observable<boolean> {
        return this.httpAuth
            .post(`${this.url}/update`, JSON.stringify(settings), { headers: this.headers })
            .map(this.extractData).catch(this.handleError);
    }

    public getUserManagementSettings(): Observable<IUserManagementSettings> {
        return this.httpAuth.get(`${this.url}/UserManagement`).map(this.extractData).catch(this.handleError);
    }

    public saveUserManagementSettings(settings: IUserManagementSettings): Observable<boolean> {
        return this.httpAuth
            .post(`${this.url}/UserManagement`, JSON.stringify(settings), { headers: this.headers })
            .map(this.extractData).catch(this.handleError);
    }
}
