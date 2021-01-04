import { Component, OnInit } from "@angular/core";

import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { NotificationService, SettingsService, JobService } from "../../services";

@Component({
    templateUrl: "./jobs.component.html",
    styleUrls: ["./jobs.component.scss"]
})
export class JobsComponent implements OnInit {

    public form: FormGroup;

    public profilesRunning: boolean;

    constructor(private readonly settingsService: SettingsService,
                private readonly fb: FormBuilder,
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
                autoDeleteRequests:     [x.autoDeleteRequests, Validators.required]
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

    public onSubmit(form: FormGroup) {
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
