import { ChangeDetectionStrategy, Component, OnInit } from "@angular/core";
import { ControlContainer, UntypedFormGroup, Validators } from "@angular/forms";
import { finalize, map } from "rxjs";

import { IMinimumAvailability, IRadarrProfile, IRadarrRootFolder, IRadarrSettings, ITag } from "../../../interfaces";
import { TesterService, NotificationService, RadarrService } from "../../../services";


@Component({
    selector: "ombi-settings-radarr-form",
    templateUrl: "./radarr-form.component.html",
    styleUrls: ["./radarr-form.component.scss"],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class RadarrFormComponent implements OnInit {

    public qualities: IRadarrProfile[];
    public rootFolders: IRadarrRootFolder[];
    public minimumAvailabilityOptions: IMinimumAvailability[];
    public tags: ITag[];
    public profilesRunning: boolean;
    public rootFoldersRunning: boolean;
    public tagsRunning: boolean;
    public form: UntypedFormGroup;

    constructor(private radarrService: RadarrService,
                private notificationService: NotificationService,
                private testerService: TesterService,
                private controlContainer: ControlContainer) {
                }

    public ngOnInit() {
        this.form = <UntypedFormGroup>this.controlContainer.control;

        this.qualities = [];
        this.qualities.push({ name: "Please Select", id: -1 });

        this.rootFolders = [];
        this.rootFolders.push({ path: "Please Select", id: -1 });

        this.tags = [];
        this.tags.push({ label: "None", id: -1 });

        this.minimumAvailabilityOptions = [
            { name: "Announced", value: "Announced" },
            { name: "In Cinemas", value: "InCinemas" },
            { name: "Physical / Web", value: "Released" },
        ];

        if (this.form.controls.defaultQualityProfile.value) {
            this.getProfiles(this.form);
        }

        if (this.form.controls.defaultRootPath.value) {
            this.getRootFolders(this.form);
        }

        if (this.form.controls.tag.value) {
            this.getTags(this.form);
        }

        this.toggleValidators();
    }

    public toggleValidators() {
        debugger;
        const enabled = this.form.controls.enabled.value as boolean;
        this.form.controls.apiKey.setValidators(enabled ? [Validators.required] : null);
        this.form.controls.defaultQualityProfile.setValidators(enabled ? [Validators.required] : null);
        this.form.controls.defaultRootPath.setValidators(enabled ? [Validators.required] : null);
        this.form.controls.ip.setValidators(enabled ? [Validators.required] : null);
        this.form.controls.port.setValidators(enabled ? [Validators.required] : null);
        this.form.controls.minimumAvailability.setValidators(enabled ? [Validators.required] : null);
    }

    public getProfiles(form: UntypedFormGroup) {
         this.profilesRunning = true;
         this.radarrService.getQualityProfiles(form.value).subscribe(x => {
             this.qualities = x;
             this.qualities.unshift({ name: "Please Select", id: -1 });

             this.profilesRunning = false;
             this.notificationService.success("Successfully retrieved the Quality Profiles");
         });
    }

    public getRootFolders(form: UntypedFormGroup) {
         this.rootFoldersRunning = true;
         this.radarrService.getRootFolders(form.value).subscribe(x => {
             this.rootFolders = x;
             this.rootFolders.unshift({ path: "Please Select", id: -1 });

             this.rootFoldersRunning = false;
             this.notificationService.success("Successfully retrieved the Root Folders");
         });
    }

    public getTags(form: UntypedFormGroup) {
        this.tagsRunning = true;
        this.radarrService.getTagsWithSettings(form.value).pipe(
            finalize(() => {
                this.tagsRunning = false;
                this.tags.unshift({ label: "None", id: -1 });
                this.notificationService.success("Successfully retrieved the Tags");
            }),
            map(result => {
                this.tags = result;
            })
        ).subscribe()
    }

    public test(form: UntypedFormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }
        const settings = <IRadarrSettings> form.value;
        this.testerService.radarrTest(settings).subscribe(result => {
            if (result.isValid) {
                this.notificationService.success("Successfully connected to Radarr!");
            } else if (result.expectedSubDir) {
                this.notificationService.error("Your Radarr Base URL must be set to " + result.expectedSubDir);
            } else {
                this.notificationService.error("We could not connect to Radarr!");
            }
        });
    }
}
