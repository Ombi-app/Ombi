﻿import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";

import { IMattermostNotifcationSettings, INotificationTemplates, NotificationType } from "../../interfaces";
import { TesterService } from "../../services";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";

@Component({
    templateUrl: "./mattermost.component.html",
})
export class MattermostComponent implements OnInit {

    public NotificationType = NotificationType;
    public templates: INotificationTemplates[];
    public form: FormGroup;

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private fb: FormBuilder,
                private testerService: TesterService) { }

    public ngOnInit() {
        this.settingsService.getMattermostNotificationSettings().subscribe(x => {
            this.templates = x.notificationTemplates;

            this.form = this.fb.group({
                enabled: [x.enabled],
                username: [x.username],
                webhookUrl: [x.webhookUrl, [Validators.required]],
                channel: [x.channel],
                iconUrl:[x.iconUrl],

            });
        });
    }

    public onSubmit(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Validation", "Please check your entered values");
            return;
        }

        const settings = <IMattermostNotifcationSettings>form.value;
        settings.notificationTemplates = this.templates;

        this.settingsService.saveMattermostNotificationSettings(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Settings Saved", "Successfully saved the Mattermost settings");
            } else {
                this.notificationService.success("Settings Saved", "There was an error when saving the Mattermost settings");
            }
        });

    }

    public test(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Validation", "Please check your entered values");
            return;
        }

        this.testerService.mattermostTest(form.value).subscribe(x => {
            if (x) {
                this.notificationService.success("Successful", "Successfully sent a Mattermost message, please check the discord channel");
            } else {
                this.notificationService.success("Error", "There was an error when sending the Mattermost message. Please check your settings");
            }
        });

    }
}
