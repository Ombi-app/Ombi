import { PlatformLocation, APP_BASE_HREF } from "@angular/common";
import { Injectable, Inject } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

import { ServiceHelpers } from "../service.helpers";

import {
    ICouchPotatoSettings,
    IDiscordNotifcationSettings,
    IEmailNotificationSettings,
    IEmbyServer,
    IJellyfinServer,
    IGotifyNotificationSettings,
    ILidarrSettings,
    IMattermostNotifcationSettings,
    IMobileNotificationTestSettings,
    INewsletterNotificationSettings,
    IPlexServer,
    IPushbulletNotificationSettings,
    IPushoverNotificationSettings,
    IRadarrSettings,
    ISickRageSettings,
    ISlackNotificationSettings,
    ISonarrSettings,
    ITelegramNotifcationSettings,
    IWebhookNotificationSettings,
    IWhatsAppSettings,
} from "../../interfaces";

@Injectable()
export class TesterService extends ServiceHelpers {
    constructor(http: HttpClient, @Inject(APP_BASE_HREF) href:string) {
        super(http, "/api/v1/tester/", href);
    }

    public discordTest(settings: IDiscordNotifcationSettings): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}discord`, JSON.stringify(settings),  {headers: this.headers});
    }

    public pushbulletTest(settings: IPushbulletNotificationSettings): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}pushbullet`, JSON.stringify(settings),  {headers: this.headers});
    }

    public pushoverTest(settings: IPushoverNotificationSettings): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}pushover`, JSON.stringify(settings), { headers: this.headers });
    }

    public gotifyTest(settings: IGotifyNotificationSettings): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}gotify`, JSON.stringify(settings), { headers: this.headers });
    }

    public webhookTest(settings: IWebhookNotificationSettings): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}webhook`, JSON.stringify(settings), { headers: this.headers });
    }

    public mattermostTest(settings: IMattermostNotifcationSettings): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}mattermost`, JSON.stringify(settings),  {headers: this.headers});
    }

    public whatsAppTest(settings: IWhatsAppSettings): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}whatsapp`, JSON.stringify(settings),  {headers: this.headers});
    }

    public slackTest(settings: ISlackNotificationSettings): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}slack`, JSON.stringify(settings),  {headers: this.headers});
    }

    public emailTest(settings: IEmailNotificationSettings): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}email`, JSON.stringify(settings),  {headers: this.headers});
    }

    public plexTest(settings: IPlexServer): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}plex`, JSON.stringify(settings),  {headers: this.headers});
    }

    public embyTest(settings: IEmbyServer): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}emby`, JSON.stringify(settings),  {headers: this.headers});
    }

    public jellyfinTest(settings: IJellyfinServer): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}jellyfin`, JSON.stringify(settings),  {headers: this.headers});
    }

    public radarrTest(settings: IRadarrSettings): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}radarr`, JSON.stringify(settings),  {headers: this.headers});
    }

    public lidarrTest(settings: ILidarrSettings): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}lidarr`, JSON.stringify(settings),  {headers: this.headers});
    }

    public sonarrTest(settings: ISonarrSettings): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}sonarr`, JSON.stringify(settings),  {headers: this.headers});
    }

    public couchPotatoTest(settings: ICouchPotatoSettings): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}couchpotato`, JSON.stringify(settings),  {headers: this.headers});
    }

    public telegramTest(settings: ITelegramNotifcationSettings): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}telegram`, JSON.stringify(settings),  {headers: this.headers});
    }

    public sickrageTest(settings: ISickRageSettings): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}sickrage`, JSON.stringify(settings),  {headers: this.headers});
    }

    public newsletterTest(settings: INewsletterNotificationSettings): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}newsletter`, JSON.stringify(settings),  {headers: this.headers});
    }

    public mobileNotificationTest(settings: IMobileNotificationTestSettings): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}mobile`, JSON.stringify(settings),  {headers: this.headers});
    }
}
