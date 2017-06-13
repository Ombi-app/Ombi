import { Component, OnInit } from '@angular/core';
import { Subject } from 'rxjs/Subject';
import "rxjs/add/operator/takeUntil";

import { IRadarrSettings } from '../../interfaces/ISettings';
import { IRadarrProfile, IRadarrRootFolder } from '../../interfaces/IRadarr';
import { SettingsService } from '../../services/settings.service';
import { RadarrService } from '../../services/applications/radarr.service';
import { NotificationService } from "../../services/notification.service";

@Component({
    selector: 'ombi',
    templateUrl: './radarr.component.html',
})
export class RadarrComponent implements OnInit {

    constructor(private settingsService: SettingsService, private radarrService: RadarrService, private notificationService: NotificationService) { }

    settings: IRadarrSettings;

    qualities: IRadarrProfile[];
    rootFolders: IRadarrRootFolder[];
    
    selectedRootFolder: IRadarrRootFolder;
    selectedQuality: IRadarrProfile;

    profilesRunning: boolean;
    rootFoldersRunning: boolean;

    advanced = false;
    private subscriptions = new Subject<void>();

    ngOnInit(): void {

        this.settingsService.getRadarr()
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.settings = x;
            });
    }


    getProfiles() {
         this.profilesRunning = true;
         this.radarrService.getQualityProfiles(this.settings).subscribe(x => {
             this.qualities = x;
         
             this.profilesRunning = false;
             this.notificationService.success("Quality Profiles", "Successfully retrevied the Quality Profiles");
         });
    }

    getRootFolders() {
         this.rootFoldersRunning = true;
         this.radarrService.getRootFolders(this.settings).subscribe(x => {
             this.rootFolders = x;
         
             this.rootFoldersRunning = false;
             this.notificationService.success("Settings Saved", "Successfully retrevied the Root Folders");
         });
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