import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";

import { IMinimumAvailability, IRadarrCombined, IRadarrProfile, IRadarrRootFolder } from "../../interfaces";
import { NotificationService, SettingsService } from "../../services";
import { FeaturesFacade } from "../../state/features/features.facade";

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
    public is4kEnabled: boolean = false;

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private featureFacade: FeaturesFacade,
                private fb: FormBuilder) { }

    public ngOnInit() {
        this.is4kEnabled = this.featureFacade.is4kEnabled();
        this.settingsService.getRadarr()
            .subscribe(x => {
                this.form = this.fb.group({
                    radarr: this.fb.group({
                        enabled: [x.radarr.enabled],
                        apiKey: [x.radarr.apiKey],
                        defaultQualityProfile: [+x.radarr.defaultQualityProfile],
                        defaultRootPath: [x.radarr.defaultRootPath],
                        ssl: [x.radarr.ssl],
                        subDir: [x.radarr.subDir],
                        ip: [x.radarr.ip],
                        port: [x.radarr.port],
                        addOnly: [x.radarr.addOnly],
                        minimumAvailability: [x.radarr.minimumAvailability],
                        scanForAvailability: [x.radarr.scanForAvailability]
                    }),
                    radarr4K: this.fb.group({
                        enabled: [x.radarr4K.enabled],
                        apiKey: [x.radarr4K.apiKey],
                        defaultQualityProfile: [+x.radarr4K.defaultQualityProfile],
                        defaultRootPath: [x.radarr4K.defaultRootPath],
                        ssl: [x.radarr4K.ssl],
                        subDir: [x.radarr4K.subDir],
                        ip: [x.radarr4K.ip],
                        port: [x.radarr4K.port],
                        addOnly: [x.radarr4K.addOnly],
                        minimumAvailability: [x.radarr4K.minimumAvailability],
                        scanForAvailability: [x.radarr4K.scanForAvailability]
                    }),
                });
            });
    }


    public onSubmit(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }
        const radarrForm = form.controls.radarr as FormGroup;
        const radarr4KForm = form.controls.radarr4K as FormGroup;

        if (radarrForm.controls.enabled.value && (radarrForm.controls.defaultQualityProfile.value === -1 || radarrForm.controls.defaultRootPath.value === "Please Select")) {
            this.notificationService.error("Please check your entered values for Radarr");
            return;
        }
        if (radarr4KForm.controls.enabled.value && (radarr4KForm.controls.defaultQualityProfile.value === -1 || radarr4KForm.controls.defaultRootPath.value === "Please Select")) {
            this.notificationService.error("Please check your entered values for Radarr 4K");
            return;
        }

        const settings = <IRadarrCombined> form.value;
        this.settingsService.saveRadarr(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved Radarr settings");
            } else {
                this.notificationService.success("There was an error when saving the Radarr settings");
            }
        });

    }
}
