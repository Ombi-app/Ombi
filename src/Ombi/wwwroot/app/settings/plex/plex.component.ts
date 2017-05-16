import { Component, OnInit } from '@angular/core';

import { IPlexSettings, IPlexLibraries, IPlexServer } from '../../interfaces/ISettings'


import { SettingsService } from '../../services/settings.service';
import { PlexService } from '../../services/applications/plex.service';
import { NotificationService } from "../../services/notification.service";

@Component({
    selector: 'ombi',
    moduleId: module.id,
    templateUrl: './plex.component.html',
})
export class PlexComponent implements OnInit {

    constructor(private settingsService: SettingsService, private notificationService: NotificationService, private plexService: PlexService) { }

    settings: IPlexSettings;
    username: string;
    password: string;

    ngOnInit(): void {
        this.settingsService.getPlex().subscribe(x => {
            this.settings = x;
        }
        );
    }

    requestToken() {
        // TODO Plex Service
    }

    testPlex() {
        // TODO Plex Service
    }

    addTab() {
        //this.settings.servers.push(<IPlexServer>{ name: "New*", id: Math.floor(Math.random() * (99999 - 0 + 1) + 1) });

        this.notificationService.warning("Disabled", "This feature is currently disabled");
    }

    removeServer(server: IPlexServer) {

        this.notificationService.warning("Disabled", "This feature is currently disabled");
        //var index = this.settings.servers.indexOf(server, 0);
        //if (index > -1) {
        //    this.settings.servers.splice(index, 1);
        //}
    }

    loadLibraries(server:IPlexServer) {
        this.plexService.getLibraries(this.settings).subscribe(x => {

            server.plexSelectedLibraries = [];
            x.mediaContainer.directory.forEach((item, index) => {
                var lib: IPlexLibraries = {
                    key: item.key,
                    title: item.title,
                    enabled: false
                };
                server.plexSelectedLibraries.push(lib);

            });
        });
    }

    save() {
        var filtered = this.settings.servers.filter(x => x.name !== "");
        this.settings.servers = filtered;
        this.settingsService.savePlex(this.settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Settings Saved", "Successfully saved Plex settings");
            } else {
                this.notificationService.success("Settings Saved", "There was an error when saving the Plex settings");
            }
        });
    }
}