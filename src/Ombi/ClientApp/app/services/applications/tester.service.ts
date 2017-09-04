import { Injectable } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';
import { Observable } from 'rxjs/Rx';

import { ServiceAuthHelpers } from '../service.helpers';

import {
    IDiscordNotifcationSettings,
    IEmailNotificationSettings,
    IPushbulletNotificationSettings,
    ISlackNotificationSettings,
    IPushoverNotificationSettings,
    IMattermostNotifcationSettings
} from '../../interfaces/INotifcationSettings'

import {
    IEmbyServer,
    IPlexServer,
    IRadarrSettings,
    ISonarrSettings
} from '../../interfaces/ISettings';

@Injectable()
export class TesterService extends ServiceAuthHelpers {
    constructor(http: AuthHttp) {
        super(http, '/api/v1/tester/');
    }

    discordTest(settings: IDiscordNotifcationSettings): Observable<boolean> {
        return this.http.post(`${this.url}discord`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }

    pushbulletTest(settings: IPushbulletNotificationSettings): Observable<boolean> {
        return this.http.post(`${this.url}pushbullet`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }
    pushoverTest(settings: IPushoverNotificationSettings): Observable<boolean> {
        return this.http.post(`${this.url}pushover`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }

    mattermostTest(settings: IMattermostNotifcationSettings): Observable<boolean> {
        return this.http.post(`${this.url}mattermost`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }

    slackTest(settings: ISlackNotificationSettings): Observable<boolean> {
        return this.http.post(`${this.url}slack`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }

    emailTest(settings: IEmailNotificationSettings): Observable<boolean> {
        return this.http.post(`${this.url}email`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }
    plexTest(settings: IPlexServer): Observable<boolean> {
        return this.http.post(`${this.url}plex`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }
    embyTest(settings: IEmbyServer): Observable<boolean> {
        return this.http.post(`${this.url}emby`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }
    radarrTest(settings: IRadarrSettings): Observable<boolean> {
        return this.http.post(`${this.url}radarr`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }
    sonarrTest(settings: ISonarrSettings): Observable<boolean> {
        return this.http.post(`${this.url}sonarr`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }   
}