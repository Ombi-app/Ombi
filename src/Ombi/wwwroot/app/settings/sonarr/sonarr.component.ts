import { Component, OnInit } from '@angular/core';

import { ISonarrSettings } from '../../interfaces/ISettings'
import { ISonarrProfile, ISonarrRootFolder } from '../../interfaces/ISonarr'
import { SettingsService } from '../../services/settings.service';
import { SonarrService } from '../../services/applications/sonarr.service';
import { NotificationService } from "../../services/notification.service";

@Component({
    selector: 'ombi',
    moduleId: module.id,
    templateUrl: './sonarr.component.html',
})
export class SonarrComponent implements OnInit {

    constructor(private settingsService: SettingsService, private sonarrService: SonarrService, private notificationService: NotificationService) { }

    settings: ISonarrSettings;

    qualities: ISonarrProfile[];
    rootFolders: ISonarrRootFolder[];

    selectedRootFolder:ISonarrRootFolder;
    selectedQuality: ISonarrProfile;

    profilesRunning: boolean;
    rootFoldersRunning:boolean;

    ngOnInit(): void {
        this.settings = {
            apiKey: "",
            port: 8081,
            fullRootPath: "",
            rootPath: "",
            subDir: "",
            ssl: false,
            seasonFolders: false,
            qualityProfile: "",
            ip: "",
            enable: false,
            id: 0
        };
    }


    getProfiles() {
        this.profilesRunning = true;
        this.sonarrService.getQualityProfiles(this.settings).subscribe(x => {
            this.qualities = x;

            this.profilesRunning = false;
            this.notificationService.success("Quality Profiles", "Successfully retrevied the Quality Profiles");
        });
    }

    getRootFolders() {
        this.rootFoldersRunning = true;
        this.sonarrService.getRootFolders(this.settings).subscribe(x => {
            this.rootFolders = x;

            this.rootFoldersRunning = false;
            this.notificationService.success("Settings Saved", "Successfully retrevied the Root Folders");
        });
    }

    test() {
        // TODO
    }

    save() {
        this.settingsService.saveSonarr(this.settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Settings Saved", "Successfully saved Ombi settings");
            } else {
                this.notificationService.success("Settings Saved", "There was an error when saving the Ombi settings");
            }
        });
    }
}