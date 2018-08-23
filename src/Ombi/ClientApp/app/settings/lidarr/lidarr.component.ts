import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";

import { ILidarrSettings, IMinimumAvailability, IRadarrProfile, IRadarrRootFolder } from "../../interfaces";
import { IRadarrSettings } from "../../interfaces";
import { LidarrService, TesterService } from "../../services";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";

@Component({
    templateUrl: "./Lidarr.component.html",
})
export class LidarrComponent implements OnInit {

    public qualities: IRadarrProfile[];
    public rootFolders: IRadarrRootFolder[];
    public minimumAvailabilityOptions: IMinimumAvailability[];
    public profilesRunning: boolean;
    public rootFoldersRunning: boolean;
    public advanced = false;
    public form: FormGroup;

    constructor(private settingsService: SettingsService,
                private lidarrService: LidarrService,
                private notificationService: NotificationService,
                private fb: FormBuilder,
                private testerService: TesterService) { }

    public ngOnInit() {
        this.settingsService.getLidarr()
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
                });

                if (x.defaultQualityProfile) {
                    this.getProfiles(this.form);
                }
                if (x.defaultRootPath) {
                    this.getRootFolders(this.form);
                }
            });

        this.qualities = [];
        this.qualities.push({ name: "Please Select", id: -1 });
        
        this.rootFolders = [];
        this.rootFolders.push({ path: "Please Select", id: -1 });
    }

    public getProfiles(form: FormGroup) {
         this.profilesRunning = true;
         this.lidarrService.getQualityProfiles(form.value).subscribe(x => {
             this.qualities = x;
             this.qualities.unshift({ name: "Please Select", id: -1 });

             this.profilesRunning = false;
             this.notificationService.success("Successfully retrieved the Quality Profiles");
         });
    }

    public getRootFolders(form: FormGroup) {
         this.rootFoldersRunning = true;
         this.lidarrService.getRootFolders(form.value).subscribe(x => {
             this.rootFolders = x;
             this.rootFolders.unshift({ path: "Please Select", id: -1 });

             this.rootFoldersRunning = false;
             this.notificationService.success("Successfully retrieved the Root Folders");
         });
    }

    public test(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }
        const settings = <IRadarrSettings>form.value;
        this.testerService.radarrTest(settings).subscribe(x => {
            if (x === true) {
                this.notificationService.success("Successfully connected to Lidarr!");
            } else {
                this.notificationService.error("We could not connect to Lidarr!");
            }
        });
    }

public onSubmit(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }
        if(form.controls.defaultQualityProfile.value === "-1" || form.controls.defaultRootPath.value === "Please Select") {
            this.notificationService.error("Please check your entered values");
            return;
        }

        const settings = <ILidarrSettings>form.value;
        this.settingsService.saveLidarr(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved Lidarr settings");
            } else {
                this.notificationService.success("There was an error when saving the Lidarr settings");
            }
        });

    }
}
