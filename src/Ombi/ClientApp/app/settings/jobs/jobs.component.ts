import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { NotificationService, SettingsService } from "../../services";

@Component({
    templateUrl: "./jobs.component.html",
})
export class JobsComponent implements OnInit {

    public form: FormGroup;
    
    public profilesRunning: boolean;
    
    constructor(private readonly settingsService: SettingsService,
                private readonly fb: FormBuilder,
                private readonly notificationService: NotificationService) { }
    
    public ngOnInit() {
        this.settingsService.getJobSettings().subscribe(x => {
            this.form = this.fb.group({
                automaticUpdater:         [x.automaticUpdater, Validators.required],
                couchPotatoSync:          [x.couchPotatoSync, Validators.required],
                embyContentSync:          [x.embyContentSync, Validators.required],
                plexContentSync:          [x.plexContentSync, Validators.required],
                userImporter:             [x.userImporter, Validators.required],
                sonarrSync:                   [x.radarrSync, Validators.required],
                radarrSync:               [x.sonarrSync, Validators.required],
            });
        });
    }
    
    public onSubmit(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }
        const settings = form.value;
        this.settingsService.saveJobSettings(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved the job settings");
            } else {
                this.notificationService.success("There was an error when saving the job settings");
            }
        });
    }
}
