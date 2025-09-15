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
import { UntypedFormBuilder, UntypedFormGroup } from "@angular/forms";

import { NotificationService } from "../../services";
import { JobService, SettingsService } from "../../services";

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
    templateUrl: "./update.component.html",
    styleUrls: ["./update.component.scss"]
})
export class UpdateComponent implements OnInit {

    public form: UntypedFormGroup;
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
                private fb: UntypedFormBuilder) { }

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

    public onSubmit(form: UntypedFormGroup) {
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
