import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";

import { IMinimumAvailability, IRadarrCombined, IRadarrProfile, IRadarrRootFolder } from "../../interfaces";
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
        if (radarrForm.controls.enabled && (radarrForm.controls.defaultQualityProfile.value === "-1" || radarrForm.controls.defaultRootPath.value === "Please Select")) {
            this.notificationService.error("Please check your entered values for Radarr");
            return;
        }
        if (radarr4KForm.controls.enabled && (radarr4KForm.controls.defaultQualityProfile.value === "-1" || radarr4KForm.controls.defaultRootPath.value === "Please Select")) {
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
