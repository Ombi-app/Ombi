import { Component, OnInit } from '@angular/core';
import { FormGroup, Validators, FormBuilder } from '@angular/forms';

import { INotificationTemplates, IPushoverNotificationSettings, NotificationType } from '../../interfaces/INotifcationSettings';
import { SettingsService } from '../../services/settings.service';
import { NotificationService } from "../../services/notification.service";
import { TesterService } from "../../services/applications/tester.service";

@Component({
    templateUrl: './pushover.component.html',
})
export class PushoverComponent implements OnInit {
    constructor(private settingsService: SettingsService,
        private notificationService: NotificationService,
        private fb: FormBuilder,
        private testerService : TesterService) { }

    NotificationType = NotificationType;
    templates: INotificationTemplates[];

    form: FormGroup;

    ngOnInit(): void {
        this.settingsService.getPushoverNotificationSettings().subscribe(x => {
            this.templates = x.notificationTemplates;

            this.form = this.fb.group({
                enabled: [x.enabled],
                userToken: [x.userToken],
                accessToken: [x.accessToken, [Validators.required]],
            });
        });
    }

    onSubmit(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Validation", "Please check your entered values");
            return;
        }

        var settings = <IPushoverNotificationSettings>form.value;
        settings.notificationTemplates = this.templates;

        this.settingsService.savePushoverNotificationSettings(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Settings Saved", "Successfully saved the Pushover settings");
            } else {
                this.notificationService.success("Settings Saved", "There was an error when saving the Pushover settings");
            }
        });

    }

    test(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Validation", "Please check your entered values");
            return;
        }

        this.testerService.pushoverTest(form.value).subscribe(x => {
            if (x) {
                this.notificationService.success("Successful", "Successfully sent a Pushbullet message, please check the discord channel");
            } else {
                this.notificationService.success("Error", "There was an error when sending the Pushbullet message. Please check your settings");
            }
        })

    }
}