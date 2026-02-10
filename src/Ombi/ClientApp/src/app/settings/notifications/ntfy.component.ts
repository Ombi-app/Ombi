import { Component, OnInit } from "@angular/core";
import { ReactiveFormsModule, UntypedFormBuilder, UntypedFormGroup, Validators } from "@angular/forms";

import { INtfyNotificationSettings, INotificationTemplates, NotificationType } from "../../interfaces";
import { TesterService } from "../../services";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";
import { CommonModule } from "@angular/common";
import { MatButtonModule } from "@angular/material/button";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";
import { NotificationTemplate } from "./notificationtemplate.component";

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
        TranslateModule,
        NotificationTemplate
    ],
    templateUrl: "./ntfy.component.html",
    styleUrls: ["./notificationtemplate.component.scss"]
})
export class NtfyComponent implements OnInit {
    public NotificationType = NotificationType;
    public templates: INotificationTemplates[];
    public form: UntypedFormGroup;

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private fb: UntypedFormBuilder,
                private testerService: TesterService) { }

    public ngOnInit() {
        this.settingsService.getNtfyNotificationSettings().subscribe(x => {
            this.templates = x.notificationTemplates;

            this.form = this.fb.group({
                enabled: [x.enabled],
                baseUrl: [x.baseUrl, [Validators.required]],
                topic: [x.topic, [Validators.required]],
                authorizationHeader: [x.authorizationHeader],
                priority: [x.priority],
            });
        });
    }

    public onSubmit(form: UntypedFormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        const settings = <INtfyNotificationSettings> form.value;
        settings.notificationTemplates = this.templates;

        this.settingsService.saveNtfyNotificationSettings(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved the Ntfy settings");
            } else {
                this.notificationService.success("There was an error when saving the Ntfy settings");
            }
        });

    }

    public test(form: UntypedFormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        this.testerService.ntfyTest(form.value).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully sent a Ntfy message");
            } else {
                this.notificationService.error("There was an error when sending the Ntfy message. Please check your settings");
            }
        });
    }
}
