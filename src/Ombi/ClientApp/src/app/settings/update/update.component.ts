import { Component, OnInit } from "@angular/core";
import { UntypedFormBuilder, UntypedFormGroup, ReactiveFormsModule } from "@angular/forms";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { MatButtonModule } from "@angular/material/button";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";

import { IUpdateSettings, IUpdateModel } from "../../interfaces";
import { NotificationService, SettingsService } from "../../services";
import { UpdateService } from "../../services/update.service";

@Component({
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatSlideToggleModule,
        MatTooltipModule,
        TranslateModule,
    ],
    templateUrl: "./update.component.html",
    styleUrls: ["./update.component.scss"],
})
export class UpdateComponent implements OnInit {

    public form: UntypedFormGroup;
    public updateStatus: IUpdateModel;
    public currentVersion: string;
    public checkingForUpdate = false;

    public scheduleOptions = [
        { label: "Every 6 Hours", value: "0 0 0/6 1/1 * ? *" },
        { label: "Every 12 Hours", value: "0 0 0/12 1/1 * ? *" },
        { label: "Daily", value: "0 0 3 1/1 * ? *" },
        { label: "Weekly", value: "0 0 3 ? * MON *" },
    ];

    constructor(
        private settingsService: SettingsService,
        private updateService: UpdateService,
        private notificationService: NotificationService,
        private fb: UntypedFormBuilder
    ) {}

    public ngOnInit() {
        this.settingsService.getUpdateSettings().subscribe(x => {
            this.form = this.fb.group({
                autoUpdateEnabled: [x.autoUpdateEnabled],
                updateSchedule: [x.updateSchedule || this.scheduleOptions[0].value],
                processName: [x.processName],
                useScript: [x.useScript],
                scriptLocation: [x.scriptLocation],
                windowsService: [x.windowsService],
                windowsServiceName: [x.windowsServiceName],
                testMode: [x.testMode],
                isWindows: [{ value: x.isWindows, disabled: true }],
            });
        });

        this.settingsService.about().subscribe(x => {
            this.currentVersion = x.version;
        });
    }

    public checkForUpdate() {
        this.checkingForUpdate = true;
        this.updateService.checkForUpdate().subscribe(x => {
            this.updateStatus = x;
            this.checkingForUpdate = false;
            if (x.updateAvailable) {
                this.notificationService.success(`Update available: v${x.updateVersionString}`);
            } else {
                this.notificationService.success("You are running the latest version.");
            }
        });
    }

    public onSubmit(form: UntypedFormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        this.settingsService.saveUpdateSettings(form.value).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved Update settings");
            } else {
                this.notificationService.error("There was an error when saving the Update settings");
            }
        });
    }
}
