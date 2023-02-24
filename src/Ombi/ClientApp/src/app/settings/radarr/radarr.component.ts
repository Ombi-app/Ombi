import { Component, OnInit, QueryList, ViewChildren } from "@angular/core";
import { UntypedFormBuilder, UntypedFormGroup } from "@angular/forms";
import { RadarrFacade } from "app/state/radarr";

import { IMinimumAvailability, IRadarrCombined, IRadarrProfile, IRadarrRootFolder } from "../../interfaces";
import { NotificationService } from "../../services";
import { FeaturesFacade } from "../../state/features/features.facade";
import { RadarrFormComponent } from "./components/radarr-form.component";

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
    public form: UntypedFormGroup;
    public is4kEnabled: boolean = false;

    @ViewChildren('4kForm') public form4k: QueryList<RadarrFormComponent>;
    @ViewChildren('normalForm') public normalForm: QueryList<RadarrFormComponent>;

    constructor(private radarrFacade: RadarrFacade,
                private notificationService: NotificationService,
                private featureFacade: FeaturesFacade,
                private fb: UntypedFormBuilder) { }


    public ngOnInit() {
        this.is4kEnabled = this.featureFacade.is4kEnabled();
        this.radarrFacade.state$()
            .subscribe(x => {
                this.form = this.fb.group({
                    radarr: this.fb.group({
                        enabled: [x.settings.radarr.enabled],
                        apiKey: [x.settings.radarr.apiKey],
                        defaultQualityProfile: [+x.settings.radarr.defaultQualityProfile],
                        defaultRootPath: [x.settings.radarr.defaultRootPath],
                        tag: [x.settings.radarr.tag],
                        sendUserTags: [x.settings.radarr.sendUserTags],
                        ssl: [x.settings.radarr.ssl],
                        subDir: [x.settings.radarr.subDir],
                        ip: [x.settings.radarr.ip],
                        port: [x.settings.radarr.port],
                        addOnly: [x.settings.radarr.addOnly],
                        minimumAvailability: [x.settings.radarr.minimumAvailability],
                        scanForAvailability: [x.settings.radarr.scanForAvailability]
                    }),
                    radarr4K: this.fb.group({
                        enabled: [x.settings.radarr4K.enabled],
                        apiKey: [x.settings.radarr4K.apiKey],
                        defaultQualityProfile: [+x.settings.radarr4K.defaultQualityProfile],
                        defaultRootPath: [x.settings.radarr4K.defaultRootPath],
                        tag: [x.settings.radarr4K.tag],
                        sendUserTags: [x.settings.radarr4K.sendUserTags],
                        ssl: [x.settings.radarr4K.ssl],
                        subDir: [x.settings.radarr4K.subDir],
                        ip: [x.settings.radarr4K.ip],
                        port: [x.settings.radarr4K.port],
                        addOnly: [x.settings.radarr4K.addOnly],
                        minimumAvailability: [x.settings.radarr4K.minimumAvailability],
                        scanForAvailability: [x.settings.radarr4K.scanForAvailability]
                    }),
                });
                this.normalForm.changes.forEach((comp => {
                    comp.first.toggleValidators();
                }))
                if (this.is4kEnabled) {
                    this.form4k.changes.forEach((comp => {
                        comp.first.toggleValidators();
                    }))
                }
            });

    }


    public onSubmit(form: UntypedFormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }
        const radarrForm = form.controls.radarr as UntypedFormGroup;
        const radarr4KForm = form.controls.radarr4K as UntypedFormGroup;

        if (radarrForm.controls.enabled.value && (radarrForm.controls.defaultQualityProfile.value === -1
            || radarrForm.controls.defaultRootPath.value === "Please Select")) {
            this.notificationService.error("Please check your entered values for Radarr");
            return;
        }
        if (radarr4KForm.controls.enabled.value && (radarr4KForm.controls.defaultQualityProfile.value === -1
            || radarr4KForm.controls.defaultRootPath.value === "Please Select")) {
            this.notificationService.error("Please check your entered values for Radarr 4K");
            return;
        }

        if (radarr4KForm.controls.tag.value === -1) {
            radarr4KForm.controls.tag.setValue(null);
        }
        if (radarrForm.controls.tag.value === -1) {
            radarr4KForm.controls.tag.setValue(null);
        }

        const settings = <IRadarrCombined> form.value;
        this.radarrFacade.updateSettings(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved Radarr settings");
            } else {
                this.notificationService.success("There was an error when saving the Radarr settings");
            }
        });

    }
}
