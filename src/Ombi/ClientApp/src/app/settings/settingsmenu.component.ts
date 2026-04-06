import { Component, OnInit, OnDestroy } from "@angular/core";
import { CommonModule } from "@angular/common";
import { Router, RouterModule, NavigationEnd } from "@angular/router";
import { MatButtonModule } from "@angular/material/button";
import { MatMenuModule } from "@angular/material/menu";
import { Subscription } from "rxjs";
import { filter } from "rxjs/operators";

@Component({
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        MatButtonModule,
        MatMenuModule,
    ],
    selector: "settings-menu",
    templateUrl: "./settingsmenu.component.html",
    styleUrls: ["./settingsmenu.component.scss"]
})
export class SettingsMenuComponent implements OnInit, OnDestroy {
    public activeGroup: string = '';
    private routerSub: Subscription;

    private readonly groupMap: Record<string, string> = {
        'ombi': 'configuration',
        'features': 'configuration',
        'customization': 'configuration',
        'landingpage': 'configuration',
        'issues': 'configuration',
        'usermanagement': 'configuration',
        'authentication': 'configuration',
        'themoviedb': 'configuration',
        'plex': 'media-servers',
        'emby': 'media-servers',
        'jellyfin': 'media-servers',
        'sonarr': 'media-management',
        'sickrage': 'media-management',
        'radarr': 'media-management',
        'couchpotato': 'media-management',
        'lidarr': 'media-management',
        'cloudmobile': 'notifications',
        'email': 'notifications',
        'massemail': 'notifications',
        'newsletter': 'notifications',
        'discord': 'notifications',
        'slack': 'notifications',
        'pushbullet': 'notifications',
        'pushover': 'notifications',
        'mattermost': 'notifications',
        'telegram': 'notifications',
        'gotify': 'notifications',
        'ntfy': 'notifications',
        'twilio': 'notifications',
        'webhook': 'notifications',
        'about': 'system',
        'failedrequests': 'system',
        'jobs': 'system',
        'logs': 'system',
    };

    constructor(private router: Router) {}

    public ngOnInit() {
        const element = document.getElementById("settings");
        if (element != null) {
            element.classList.add("active");
        }

        this.updateActiveGroup(this.router.url);
        this.routerSub = this.router.events.pipe(
            filter(event => event instanceof NavigationEnd)
        ).subscribe((event: NavigationEnd) => {
            this.updateActiveGroup(event.urlAfterRedirects || event.url);
        });
    }

    public ngOnDestroy() {
        const element = document.getElementById("settings");
        if (element != null) {
            element.classList.remove("active");
        }
        if (this.routerSub) {
            this.routerSub.unsubscribe();
        }
    }

    private updateActiveGroup(url: string) {
        const segment = url.split('/').pop()?.toLowerCase() || '';
        this.activeGroup = this.groupMap[segment] || 'system';
    }
}
