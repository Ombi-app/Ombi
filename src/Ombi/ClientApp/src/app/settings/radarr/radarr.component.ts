import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";

import { IMinimumAvailability, IRadarrProfile, IRadarrRootFolder } from "../../interfaces";
import { IRadarrSettings } from "../../interfaces";
import { RadarrService } from "../../services";
import { TesterService } from "../../services";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";

@Component({
    templateUrl: "./radarr.component.html",
    styleUrls: ["./radarr.component.scss"]
})
export class RadarrComponent implements OnInit {

    public qualities: IRadarrProfile[];
    public rootFolders: IRadarrRootFolder[];
    public minimumAvailabilityOptions: IMinimumAvailability[];
    public profilesRunning: boolean;
    public rootFoldersRunning: boolean;
    public form: FormGroup;

    constructor(private settingsService: SettingsService,
                private radarrService: RadarrService,
                private notificationService: NotificationService,
                private fb: FormBuilder,
                private testerService: TesterService) { }

    public ngOnInit() {
        this.settingsService.getRadarr()
            .subscribe(x => {

                this.form = this.fb.group({
                    enabled: [x.enabled],
                    apiKey: [x.apiKey, [Validators.required]],
                    defaultQualityProfile: [+x.defaultQualityProfile, [Validators.required]],
                    defaultRootPath: [x.defaultRootPath, [Validators.required]],
                    ssl: [x.ssl],
                    subDir: [x.subDir],
                    ip: [x.ip, [Validators.required]],
                    port: [x.port, [Validators.required]],
                    addOnly: [x.addOnly],
                    minimumAvailability: [x.minimumAvailability, [Validators.required]],
                    scanForAvailability: [x.scanForAvailability],
                    v3: [x.v3]
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
        this.minimumAvailabilityOptions = [
            { name: "Announced", value: "Announced" },
            { name: "In Cinemas", value: "InCinemas" },
            { name: "Physical / Web", value: "Released" },
            { name: "PreDb", value: "PreDb" },
        ];

    }

    public getProfiles(form: FormGroup) {
         this.profilesRunning = true;
         this.radarrService.getQualityProfiles(form.value).subscribe(x => {
             this.qualities = x;
             this.qualities.unshift({ name: "Please Select", id: -1 });

             this.profilesRunning = false;
             this.notificationService.success("Successfully retrieved the Quality Profiles");
         });
    }

    public getRootFolders(form: FormGroup) {
         this.rootFoldersRunning = true;
         this.radarrService.getRootFolders(form.value).subscribe(x => {
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
        const settings = <IRadarrSettings> form.value;
        this.testerService.radarrTest(settings).subscribe(x => {
            if (x === true) {
                this.notificationService.success("Successfully connected to Radarr!");
            } else {
                this.notificationService.error("We could not connect to Radarr!");
            }
        });
    }

public onSubmit(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }
        if (form.controls.defaultQualityProfile.value === "-1" || form.controls.defaultRootPath.value === "Please Select") {
            this.notificationService.error("Please check your entered values");
            return;
        }

        const settings = <IRadarrSettings> form.value;
        this.settingsService.saveRadarr(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved Radarr settings");
            } else {
                this.notificationService.success("There was an error when saving the Radarr settings");
            }
        });

    }
}
