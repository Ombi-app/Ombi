import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";

import { NotificationService } from "../../services";
import { SettingsService, UpdateService } from "../../services";

@Component({
    templateUrl: "./update.component.html",
})
export class UpdateComponent implements OnInit {

    public form: FormGroup;
    public updateAvailable = false;

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private updateService: UpdateService,
                private fb: FormBuilder) { }

    public ngOnInit() {
        this.settingsService.getUpdateSettings()
            .subscribe(x => {
                this.form = this.fb.group({
                    autoUpdateEnabled: [x.autoUpdateEnabled],
                });
            });
    }

    public checkForUpdate() {
        this.updateService.checkForNewUpdate().subscribe(x => {
            if (x === true) {
                this.updateAvailable = true;
                this.notificationService.success("Update", "There is a new update available");
            } else {
                this.notificationService.success("Update", "You are on the latest version!")
            }
        });
    }

    public update() {
        this.updateService.forceUpdate().subscribe();
        this.notificationService.success("Update", "We triggered the update job");
    }

    public onSubmit(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Validation", "Please check your entered values");
            return;
        }
        this.settingsService.saveUpdateSettings(form.value)
            .subscribe(x => {
                if (x) {
                    this.notificationService.success("Settings Saved", "Successfully saved Update settings");
                } else {
                    this.notificationService.error("Settings Saved", "There was an error when saving the Update settings");
                }
            });
    }
}
