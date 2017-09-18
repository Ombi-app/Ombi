import { Component, OnDestroy, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import "rxjs/add/operator/takeUntil";
import { Subject } from "rxjs/Subject";

import { ISonarrProfile, ISonarrRootFolder } from "../../interfaces";

import { ISonarrSettings } from "../../interfaces";
import { SonarrService } from "../../services";
import { TesterService } from "../../services";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";

@Component({
    templateUrl: "./sonarr.component.html",
})
export class SonarrComponent implements OnInit, OnDestroy {

    public qualities: ISonarrProfile[];
    public rootFolders: ISonarrRootFolder[];
    public selectedRootFolder: ISonarrRootFolder;
    public selectedQuality: ISonarrProfile;
    public profilesRunning: boolean;
    public rootFoldersRunning: boolean;
    public form: FormGroup;
    public advanced = false;

    private subscriptions = new Subject<void>();

    constructor(private settingsService: SettingsService,
                private sonarrService: SonarrService,
                private notificationService: NotificationService,
                private testerService: TesterService,
                private fb: FormBuilder) { }

    public ngOnInit() {

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

                if (x.qualityProfile) {
                    this.getProfiles(this.form);
                }
                if (x.rootPath) {
                    this.getRootFolders(this.form);
                }
            });
    }

    public getProfiles(form: FormGroup) {
        this.profilesRunning = true;
        this.sonarrService.getQualityProfiles(form.value)
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.qualities = x;

                this.profilesRunning = false;
                this.notificationService.success("Quality Profiles", "Successfully retrevied the Quality Profiles");
            });
    }

    public getRootFolders(form: FormGroup) {
        this.rootFoldersRunning = true;
        this.sonarrService.getRootFolders(form.value)
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.rootFolders = x;

                this.rootFoldersRunning = false;
                this.notificationService.success("Settings Saved", "Successfully retrevied the Root Folders");
            });
    }

    public test(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Validation", "Please check your entered values");
            return;
        }
        const settings = <ISonarrSettings>form.value;
        this.testerService.sonarrTest(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Connected", "Successfully connected to Sonarr!");
            } else {
                this.notificationService.error("Connected", "We could not connect to Sonarr!");
            }
        });
    }

    public onSubmit(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Validation", "Please check your entered values");
            return;
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

    public ngOnDestroy() {
        this.subscriptions.next();
        this.subscriptions.complete();
    }
}
