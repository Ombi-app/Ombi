import { Component, OnInit } from '@angular/core';

import { IPlexSettings, IPlexLibraries } from '../../interfaces/ISettings'


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
        this.settingsService.getPlex().subscribe(x => this.settings = x);
    }

    requestToken() {
        // TODO Plex Service
    }

    testPlex() {
        // TODO Plex Service
    }

    loadLibraries() {
        this.plexService.getLibraries(this.settings).subscribe(x => {

            this.settings.plexSelectedLibraries = [];
            x.mediaContainer.directory.forEach((item, index) => {
                var lib: IPlexLibraries = {
                    key: item.key,
                    title: item.title,
                    enabled: false
                };
                this.settings.plexSelectedLibraries.push(lib);

            });
        });
    }

    save() {
        this.settingsService.savePlex(this.settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Settings Saved", "Successfully saved Plex settings");
            } else {
                this.notificationService.success("Settings Saved", "There was an error when saving the Plex settings");
            }
        });
    }
}