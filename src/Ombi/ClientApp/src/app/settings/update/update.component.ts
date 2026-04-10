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
import { finalize } from "rxjs/operators";

import { IUpdateSettings, IUpdateModel, IAbout } from "../../interfaces";
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
    public about: IAbout;
    public checkingForUpdate = false;
    private settings: IUpdateSettings;

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
            this.settings = x;
            this.form = this.fb.group({
                autoUpdateEnabled: [x.autoUpdateEnabled],
                updateSchedule: [x.updateSchedule || this.scheduleOptions[0].value],
                processName: [x.processName || 'Ombi'],
                useScript: [x.useScript],
                scriptLocation: [x.scriptLocation],
                windowsService: [x.windowsService],
                windowsServiceName: [x.windowsServiceName],
                testMode: [x.testMode],
                isWindows: [{ value: x.isWindows, disabled: true }],
            });
        });

        this.updateService.checkForUpdate().subscribe(x => {
            this.updateStatus = x;
        });

        this.settingsService.about().subscribe(x => {
            this.about = x;
        });
    }

    public onAutoUpdateToggle() {
        this.saveSettings();
    }

    public checkForUpdate() {
        this.checkingForUpdate = true;
        this.updateService.checkForUpdate().pipe(
            finalize(() => this.checkingForUpdate = false)
        ).subscribe({
            next: x => {
                this.updateStatus = x;
                if (x.updateAvailable) {
                    this.notificationService.success(`Update available: v${x.updateVersionString}`);
                } else {
                    this.notificationService.success("You are running the latest version.");
                }
            },
            error: () => {
                this.notificationService.error("Unable to check for updates right now.");
            }
        });
    }

    private saveSettings() {
        const merged = { ...this.settings, ...this.form.value };
        this.settingsService.saveUpdateSettings(merged).subscribe(result => {
            if (result) {
                this.notificationService.success("Successfully saved Update settings");
            } else {
                this.notificationService.error("There was an error when saving the Update settings");
            }
        });
    }

    public onSubmit(form: UntypedFormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        this.saveSettings();
    }
}
