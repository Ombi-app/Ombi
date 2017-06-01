import { Component, OnInit } from '@angular/core';
import { Subject } from 'rxjs/Subject';
import "rxjs/add/operator/takeUntil";

import { IRadarrSettings } from '../../interfaces/ISettings'
import { ISonarrProfile, ISonarrRootFolder } from '../../interfaces/ISonarr'
import { SettingsService } from '../../services/settings.service';
// import { RadarrService } from '../../services/applications/radarr.service';
import { NotificationService } from "../../services/notification.service";

@Component({
    selector: 'ombi',
    moduleId: module.id,
    templateUrl: './radarr.component.html',
})
export class RadarrComponent implements OnInit {

    constructor(private settingsService: SettingsService, /*private radarrService: RadarrService,*/ private notificationService: NotificationService) { }

    settings: IRadarrSettings;

    qualities: ISonarrProfile[];
    rootFolders: ISonarrRootFolder[];

    selectedRootFolder:ISonarrRootFolder;
    selectedQuality: ISonarrProfile;

    profilesRunning: boolean;
    rootFoldersRunning: boolean;
    private subscriptions = new Subject<void>();

    ngOnInit(): void {

        this.settingsService.getRadarr()
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.settings = x;
            });
    }


    getProfiles() {
        // this.profilesRunning = true;
        // this.sonarrService.getQualityProfiles(this.settings).subscribe(x => {
        //     this.qualities = x;
        // 
        //     this.profilesRunning = false;
        //     this.notificationService.success("Quality Profiles", "Successfully retrevied the Quality Profiles");
        // });
    }

    getRootFolders() {
        // this.rootFoldersRunning = true;
        // this.sonarrService.getRootFolders(this.settings).subscribe(x => {
        //     this.rootFolders = x;
        // 
        //     this.rootFoldersRunning = false;
        //     this.notificationService.success("Settings Saved", "Successfully retrevied the Root Folders");
        // });
    }

    test() {
        // TODO
    }

    save() {
        this.settingsService.saveRadarr(this.settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Settings Saved", "Successfully saved Radarr settings");
            } else {
                this.notificationService.success("Settings Saved", "There was an error when saving the Radarr settings");
            }
        });
    }

    ngOnDestroy(): void {
        this.subscriptions.next();
        this.subscriptions.complete();
    }
}