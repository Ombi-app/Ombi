import { Component, OnInit } from '@angular/core';
import { Subject } from 'rxjs/Subject';
import "rxjs/add/operator/takeUntil";
import { FormGroup, Validators, FormBuilder } from '@angular/forms';

import { IRadarrSettings } from '../../interfaces/ISettings';
import { IRadarrProfile, IRadarrRootFolder, IMinimumAvailability } from '../../interfaces/IRadarr';
import { SettingsService } from '../../services/settings.service';
import { RadarrService } from '../../services/applications/radarr.service';
import { NotificationService } from "../../services/notification.service";

@Component({
  
    templateUrl: './radarr.component.html',
})
export class RadarrComponent implements OnInit {

    constructor(private settingsService: SettingsService, private radarrService: RadarrService, private notificationService: NotificationService,
        private fb: FormBuilder) { }
    qualities: IRadarrProfile[];
    rootFolders: IRadarrRootFolder[];

    minimumAvailabilityOptions: IMinimumAvailability[];

    profilesRunning: boolean;
    rootFoldersRunning: boolean;

    advanced = false;
    private subscriptions = new Subject<void>();

    form : FormGroup;
    ngOnInit(): void {

        this.settingsService.getRadarr()
            .takeUntil(this.subscriptions)
            .subscribe(x => {

                this.form = this.fb.group({
                    enabled: [x.enabled],
                    apiKey: [x.apiKey, [Validators.required]],
                    defaultQualityProfile: [x.defaultQualityProfile, [Validators.required]],
                    defaultRootPath: [x.defaultRootPath, [Validators.required]],
                    ssl: [x.ssl],
                    subDir: [x.subDir],
                    ip: [x.ip, [Validators.required]],
                    port: [x.port, [Validators.required]],
                    addOnly: [x.addOnly],
                    minimumAvailability: [x.minimumAvailability, [Validators.required]]
                });

                if (x.defaultQualityProfile) {
                    this.getProfiles(this.form);
                }
                if (x.defaultRootPath) {
                    this.getRootFolders(this.form);
                }
            });

        this.minimumAvailabilityOptions = [
            { name: "Announced", value:"Announced" },
            { name: "In Cinemas", value:"InCinemas" },
            { name: "Physical / Web", value:"Released" },
            { name: "PreDb", value:"PreDb" },
        ]

    }


    getProfiles(form: FormGroup) {
         this.profilesRunning = true;
         this.radarrService.getQualityProfiles(form.value).subscribe(x => {
             this.qualities = x;
         
             this.profilesRunning = false;
             this.notificationService.success("Quality Profiles", "Successfully retrevied the Quality Profiles");
         });
    }

    getRootFolders(form: FormGroup) {
         this.rootFoldersRunning = true;
         this.radarrService.getRootFolders(form.value).subscribe(x => {
             this.rootFolders = x;
         
             this.rootFoldersRunning = false;
             this.notificationService.success("Settings Saved", "Successfully retrevied the Root Folders");
         });
    }

    test() {
        // TODO
    }

onSubmit(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Validation", "Please check your entered values");
            return
        }

        var settings = <IRadarrSettings>form.value;
        this.settingsService.saveRadarr(settings).subscribe(x => {
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