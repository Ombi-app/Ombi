﻿import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";

import { INotificationTemplates, IPushoverNotificationSettings, NotificationType } from "../../interfaces";
import { TesterService } from "../../services";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";

@Component({
    templateUrl: "./pushover.component.html",
})
export class PushoverComponent implements OnInit {
    public NotificationType = NotificationType;
    public templates: INotificationTemplates[];
    public form: FormGroup;

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private fb: FormBuilder,
                private testerService: TesterService) { }

    public ngOnInit() {
        this.settingsService.getPushoverNotificationSettings().subscribe(x => {
            this.templates = x.notificationTemplates;

            this.form = this.fb.group({
                enabled: [x.enabled],
                userToken: [x.userToken],
                accessToken: [x.accessToken, [Validators.required]],
            });
        });
    }

    public onSubmit(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        const settings = <IPushoverNotificationSettings>form.value;
        settings.notificationTemplates = this.templates;

        this.settingsService.savePushoverNotificationSettings(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Settings Saved", "Successfully saved the Pushover settings");
            } else {
                this.notificationService.success("Settings Saved", "There was an error when saving the Pushover settings");
            }
        });

    }

    public test(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        this.testerService.pushoverTest(form.value).subscribe(x => {
            if (x) {
                this.notificationService.success("Successful", "Successfully sent a Pushover message");
            } else {
                this.notificationService.success("Error", "There was an error when sending the Pushover message. Please check your settings");
            }
        });
    }
}
