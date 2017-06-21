import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs/Subject';
import "rxjs/add/operator/takeUntil";
import { FormGroup, Validators, FormBuilder } from '@angular/forms';

import { ISonarrProfile, ISonarrRootFolder } from '../../interfaces/ISonarr'
import { SettingsService } from '../../services/settings.service';
import { SonarrService } from '../../services/applications/sonarr.service';
import { NotificationService } from "../../services/notification.service";

@Component({

    templateUrl: './sonarr.component.html',
})
export class SonarrComponent implements OnInit, OnDestroy {

    constructor(private settingsService: SettingsService, private sonarrService: SonarrService, private notificationService: NotificationService,
    private fb : FormBuilder) { }

    qualities: ISonarrProfile[];
    rootFolders: ISonarrRootFolder[];

    selectedRootFolder: ISonarrRootFolder;
    selectedQuality: ISonarrProfile;

    profilesRunning: boolean;
    rootFoldersRunning: boolean;
    private subscriptions = new Subject<void>();
    form : FormGroup;
    advanced = false;

    ngOnInit(): void {

        this.settingsService.getSonarr()
            .takeUntil(this.subscriptions)
            .subscribe(x => {

                 this.form = this.fb.group({
                    enabled: [x.enabled],
                    apiKey: [x.apiKey, [Validators.required]],
                    qualityProfile: [x.qualityProfile, [Validators.required]],
                    rootPath: [x.rootPath, [Validators.required]],
                    ssl: [x.ssl],
                    subDir: [x.subDir],
                    ip: [x.ip, [Validators.required]],
                    port: [x.port, [Validators.required]],
                    addOnly: [x.addOnly],
                    seasonFolders: [x.seasonFolders],
                });

                if (x.qualityProfile)
                {
                    this.getProfiles(this.form);
                }
                if (x.rootPath)
                {
                    this.getRootFolders(this.form);
                }
            });
    }


    getProfiles(form:FormGroup) {
        this.profilesRunning = true;
        this.sonarrService.getQualityProfiles(form.value)
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.qualities = x;

                this.profilesRunning = false;
                this.notificationService.success("Quality Profiles", "Successfully retrevied the Quality Profiles");
            });
    }

    getRootFolders(form:FormGroup) {
        this.rootFoldersRunning = true;
        this.sonarrService.getRootFolders(form.value)
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.rootFolders = x;

                this.rootFoldersRunning = false;
                this.notificationService.success("Settings Saved", "Successfully retrevied the Root Folders");
            });
    }

    test() {
        // TODO
    }

    onSubmit(form:FormGroup) {
   if (form.invalid) {
            this.notificationService.error("Validation", "Please check your entered values");
            return
        }
        this.settingsService.saveSonarr(form.value)
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                if (x) {
                    this.notificationService.success("Settings Saved", "Successfully saved Sonarr settings");
                } else {
                    this.notificationService.error("Settings Saved", "There was an error when saving the Sonarr settings");
                }
            });
    }

    ngOnDestroy(): void {
        this.subscriptions.next();
        this.subscriptions.complete();
    }
}