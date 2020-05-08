import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";

import { NotificationService } from "../../services";
import { JobService, SettingsService } from "../../services";

@Component({
    templateUrl: "./update.component.html",
    styleUrls: ["./update.component.scss"]
})
export class UpdateComponent implements OnInit {

    public form: FormGroup;
    public updateAvailable = false;
    public enableUpdateButton = false;
    public isWindows = false;
    public get useScript() {
        const control = this.form.get("useScript");
        return control!.value!;
    }

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private updateService: JobService,
                private fb: FormBuilder) { }

    public ngOnInit() {
        this.settingsService.getUpdateSettings()
            .subscribe(x => {
                this.form = this.fb.group({
                    autoUpdateEnabled: [x.autoUpdateEnabled],
                    username: [x.username],
                    password: [x.password],
                    processName: [x.processName],
                    useScript: [x.useScript],
                    scriptLocation: [x.scriptLocation],
                    windowsService: [x.windowsService],
                    windowsServiceName: [x.windowsServiceName],
                    testMode: [x.testMode],
                });
                this.isWindows = x.isWindows;
                this.enableUpdateButton = x.autoUpdateEnabled;
            });
    }

    public checkForUpdate() {
        this.updateService.checkForNewUpdate().subscribe(x => {
            if (x === true) {
                this.updateAvailable = true;
                this.notificationService.success("There is a new update available");
            } else {
                this.notificationService.success("You are on the latest version!");
            }
        });
    }

    public update() {
        this.updateService.forceUpdate().subscribe();
        this.notificationService.success("We triggered the update job");
    }

    public onSubmit(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }
        this.enableUpdateButton = form.value.autoUpdateEnabled;
        this.settingsService.saveUpdateSettings(form.value)
            .subscribe(x => {
                if (x) {
                    this.notificationService.success("Successfully saved Update settings");
                } else {
                    this.notificationService.error("There was an error when saving the Update settings");
                }
            });
    }
}
