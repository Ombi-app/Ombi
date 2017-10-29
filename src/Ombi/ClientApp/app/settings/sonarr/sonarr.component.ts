import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";

import { ISonarrProfile, ISonarrRootFolder } from "../../interfaces";

import { ISonarrSettings } from "../../interfaces";
import { SonarrService } from "../../services";
import { TesterService } from "../../services";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";

@Component({
    templateUrl: "./sonarr.component.html",
})
export class SonarrComponent implements OnInit {

    public qualities: ISonarrProfile[];
    public rootFolders: ISonarrRootFolder[];
    public selectedRootFolder: ISonarrRootFolder;
    public selectedQuality: ISonarrProfile;
    public profilesRunning: boolean;
    public rootFoldersRunning: boolean;
    public form: FormGroup;
    public advanced = false;

    constructor(private settingsService: SettingsService,
                private sonarrService: SonarrService,
                private notificationService: NotificationService,
                private testerService: TesterService,
                private fb: FormBuilder) { }

    public ngOnInit() {

        this.settingsService.getSonarr()
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

                if (x.qualityProfile) {
                    this.getProfiles(this.form);
                }
                if (x.rootPath) {
                    this.getRootFolders(this.form);
                }
            });
        this.rootFolders = [];
        this.qualities = [];
        this.rootFolders.push({ path: "Please Select", id: -1 });
        this.qualities.push({ name: "Please Select", id: -1 });
    }

    public getProfiles(form: FormGroup) {
        this.profilesRunning = true;
        this.sonarrService.getQualityProfiles(form.value)
            .subscribe(x => {
                this.qualities = x;
                this.qualities.unshift({ name: "Please Select", id: -1 });

                this.profilesRunning = false;
                this.notificationService.success("Quality Profiles", "Successfully retrieved the Quality Profiles");
            });
    }

    public getRootFolders(form: FormGroup) {
        this.rootFoldersRunning = true;
        this.sonarrService.getRootFolders(form.value)
            .subscribe(x => {
                this.rootFolders = x;
                this.rootFolders.unshift({ path: "Please Select", id: -1 });

                this.rootFoldersRunning = false;
                this.notificationService.success("Settings Saved", "Successfully retrieved the Root Folders");
            });
    }

    public test(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }
        const settings = <ISonarrSettings>form.value;
        this.testerService.sonarrTest(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Connected", "Successfully connected to Sonarr!");
            } else {
                this.notificationService.error("We could not connect to Sonarr!");
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
        this.settingsService.saveSonarr(form.value)
            .subscribe(x => {
                if (x) {
                    this.notificationService.success("Settings Saved", "Successfully saved Sonarr settings");
                } else {
                    this.notificationService.error("There was an error when saving the Sonarr settings");
                }
            });
    }
}
