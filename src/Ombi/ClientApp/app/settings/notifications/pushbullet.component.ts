import { Component, OnInit } from '@angular/core';
import { FormGroup, Validators, FormBuilder } from '@angular/forms';

import { INotificationTemplates, IPushbulletNotificationSettings, NotificationType } from '../../interfaces/INotifcationSettings';
import { SettingsService } from '../../services/settings.service';
import { NotificationService } from "../../services/notification.service";
import { TesterService } from "../../services/applications/tester.service";

@Component({
    templateUrl: './pushbullet.component.html',
})
export class PushbulletComponent implements OnInit {
    constructor(private settingsService: SettingsService,
        private notificationService: NotificationService,
        private fb: FormBuilder,
        private testerService : TesterService) { }

    NotificationType = NotificationType;
    templates: INotificationTemplates[];

    form: FormGroup;

    ngOnInit(): void {
        this.settingsService.getPushbulletNotificationSettings().subscribe(x => {
            this.templates = x.notificationTemplates;

            this.form = this.fb.group({
                enabled: [x.enabled],
                channelTag: [x.channelTag],
                accessToken: [x.accessToken, [Validators.required]],
            });
        });
    }

    onSubmit(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Validation", "Please check your entered values");
            return
        }

        var settings = <IPushbulletNotificationSettings>form.value;
        settings.notificationTemplates = this.templates;

        this.settingsService.savePushbulletNotificationSettings(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Settings Saved", "Successfully saved the Pushbullet settings");
            } else {
                this.notificationService.success("Settings Saved", "There was an error when saving the Pushbullet settings");
            }
        });

    }

    test(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Validation", "Please check your entered values");
            return
        }

        this.testerService.pushbulletTest(form.value).subscribe(x => {
            if (x) {
                this.notificationService.success("Successful", "Successfully sent a Pushbullet message, please check the discord channel");
            } else {
                this.notificationService.success("Error", "There was an error when sending the Pushbullet message. Please check your settings");
            }
        })

    }
}