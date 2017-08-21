import { Component, OnInit } from '@angular/core';
import { FormGroup, Validators, FormBuilder } from '@angular/forms';

import { INotificationTemplates, IMattermostNotifcationSettings, NotificationType } from '../../interfaces/INotifcationSettings';
import { SettingsService } from '../../services/settings.service';
import { NotificationService } from "../../services/notification.service";
import { TesterService } from "../../services/applications/tester.service";

@Component({
    templateUrl: './mattermost.component.html'
})
export class MattermostComponent implements OnInit {
    constructor(private settingsService: SettingsService,
        private notificationService: NotificationService,
        private fb: FormBuilder,
        private testerService : TesterService) { }

    NotificationType = NotificationType;
    templates: INotificationTemplates[];

    form: FormGroup;

    ngOnInit(): void {
        this.settingsService.getMattermostNotificationSettings().subscribe(x => {
            this.templates = x.notificationTemplates;

            this.form = this.fb.group({
                enabled: [x.enabled],
                username: [x.username],
                webhookUrl: [x.webhookUrl, [Validators.required]],
                channel: [x.channel],
                iconUrl:[x.iconUrl]

            });
        });
    }

    onSubmit(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Validation", "Please check your entered values");
            return;
        }

        var settings = <IMattermostNotifcationSettings>form.value;
        settings.notificationTemplates = this.templates;

        this.settingsService.saveMattermostNotificationSettings(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Settings Saved", "Successfully saved the Mattermost settings");
            } else {
                this.notificationService.success("Settings Saved", "There was an error when saving the Mattermost settings");
            }
        });

    }

    test(form: FormGroup) {
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
        })

    }
}