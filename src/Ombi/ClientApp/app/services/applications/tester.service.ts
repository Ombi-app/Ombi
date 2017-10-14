import { PlatformLocation } from "@angular/common";
import { Injectable } from "@angular/core";
import { AuthHttp } from "angular2-jwt";
import { Observable } from "rxjs/Rx";

import { ServiceAuthHelpers } from "../service.helpers";

import {
    ICouchPotatoSettings,
    IDiscordNotifcationSettings,
    IEmailNotificationSettings,
    IEmbyServer,
    IMattermostNotifcationSettings,
    IPlexServer,
    IPushbulletNotificationSettings,
    IPushoverNotificationSettings,
    IRadarrSettings,
    ISlackNotificationSettings,
    ISonarrSettings
} from "../../interfaces";

@Injectable()
export class TesterService extends ServiceAuthHelpers {
    constructor(http: AuthHttp, public platformLocation: PlatformLocation) {
        super(http, "/api/v1/tester/", platformLocation);
    }

    public discordTest(settings: IDiscordNotifcationSettings): Observable<boolean> {
        return this.http.post(`${this.url}discord`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }

    public pushbulletTest(settings: IPushbulletNotificationSettings): Observable<boolean> {
        return this.http.post(`${this.url}pushbullet`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }
    public pushoverTest(settings: IPushoverNotificationSettings): Observable<boolean> {
        return this.http.post(`${this.url}pushover`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }

    public mattermostTest(settings: IMattermostNotifcationSettings): Observable<boolean> {
        return this.http.post(`${this.url}mattermost`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }

    public slackTest(settings: ISlackNotificationSettings): Observable<boolean> {
        return this.http.post(`${this.url}slack`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }

    public emailTest(settings: IEmailNotificationSettings): Observable<boolean> {
        return this.http.post(`${this.url}email`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }
    public plexTest(settings: IPlexServer): Observable<boolean> {
        return this.http.post(`${this.url}plex`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }
    public embyTest(settings: IEmbyServer): Observable<boolean> {
        return this.http.post(`${this.url}emby`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }
    public radarrTest(settings: IRadarrSettings): Observable<boolean> {
        return this.http.post(`${this.url}radarr`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }
    public sonarrTest(settings: ISonarrSettings): Observable<boolean> {
        return this.http.post(`${this.url}sonarr`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }    
    public couchPotatoTest(settings: ICouchPotatoSettings): Observable<boolean> {
        return this.http.post(`${this.url}couchpotato`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }
}
