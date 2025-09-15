import { Component, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ReactiveFormsModule } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";
import { UntypedFormBuilder, UntypedFormGroup, Validators } from "@angular/forms";
import { JobService, NotificationService, SettingsService } from "../../services";

@Component({
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatCheckboxModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatSlideToggleModule,
        MatTooltipModule,
        TranslateModule
    ],
    templateUrl: "./jobs.component.html",
    styleUrls: ["./jobs.component.scss"]
})
export class JobsComponent implements OnInit {

    public form: UntypedFormGroup;

    public profilesRunning: boolean;

    constructor(private readonly settingsService: SettingsService,
                private readonly fb: UntypedFormBuilder,
                private readonly notificationService: NotificationService,
                private readonly jobsService: JobService) { }

    public ngOnInit() {
        this.settingsService.getJobSettings().subscribe(x => {
            this.form = this.fb.group({
                automaticUpdater:         [x.automaticUpdater, Validators.required],
                couchPotatoSync:          [x.couchPotatoSync, Validators.required],
                embyContentSync:          [x.embyContentSync, Validators.required],
                jellyfinContentSync:      [x.jellyfinContentSync, Validators.required],
                plexContentSync:          [x.plexContentSync, Validators.required],
                userImporter:             [x.userImporter, Validators.required],
                sonarrSync:               [x.sonarrSync, Validators.required],
                radarrSync:               [x.radarrSync, Validators.required],
                sickRageSync:             [x.sickRageSync, Validators.required],
                newsletter:               [x.newsletter, Validators.required],
                plexRecentlyAddedSync:    [x.plexRecentlyAddedSync, Validators.required],
                lidarrArtistSync:         [x.lidarrArtistSync, Validators.required],
                issuesPurge:              [x.issuesPurge, Validators.required],
                retryRequests:            [x.retryRequests, Validators.required],
                mediaDatabaseRefresh:     [x.mediaDatabaseRefresh, Validators.required],
                autoDeleteRequests:       [x.autoDeleteRequests, Validators.required],
                embyRecentlyAddedSync:    [x.embyRecentlyAddedSync, Validators.required],
                plexWatchlistImport:      [x.plexWatchlistImport, Validators.required],
            });
        });
    }

    public testCron(expression: string) {
        this.settingsService.testCron({ expression }).subscribe(x => {
            if(x.success) {
                this.notificationService.success("Cron is Valid");
            } else {
                this.notificationService.error(x.message);
            }
        });
    }

    public onSubmit(form: UntypedFormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }
        const settings = form.value;
        this.settingsService.saveJobSettings(settings).subscribe(x => {
            if (x.result) {
                this.notificationService.success("Successfully saved the job settings");
            } else {
                this.notificationService.error("There was an error when saving the job settings. " + x.message);
            }
        });
    }

    public runArrAvailabilityChecker() {
        this.jobsService.runArrAvailabilityChecker().subscribe();
    }
}
