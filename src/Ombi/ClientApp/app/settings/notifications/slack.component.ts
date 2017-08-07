import { Component, OnInit } from '@angular/core';
import { FormGroup, Validators, FormBuilder } from '@angular/forms';

import { INotificationTemplates, ISlackNotificationSettings, NotificationType } from '../../interfaces/INotifcationSettings';
import { SettingsService } from '../../services/settings.service';
import { NotificationService } from "../../services/notification.service";
import { TesterService } from "../../services/applications/tester.service";

@Component({
    templateUrl: './slack.component.html',
})
export class SlackComponent implements OnInit {
    constructor(private settingsService: SettingsService,
        private notificationService: NotificationService,
        private fb: FormBuilder,
        private testerService : TesterService) { }

    NotificationType = NotificationType;
    templates: INotificationTemplates[];

    form: FormGroup;

    ngOnInit(): void {
        this.settingsService.getSlackNotificationSettings().subscribe(x => {
            this.templates = x.notificationTemplates;

            this.form = this.fb.group({
                enabled: [x.enabled],
                username: [x.username],
                webhookUrl: [x.webhookUrl, [Validators.required]],
                iconEmoji: [x.iconEmoji],
                iconUrl: [x.iconUrl],
                channel: [x.channel]

            });
        });
    }

    onSubmit(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Validation", "Please check your entered values");
            return
        }
        

        var settings = <ISlackNotificationSettings>form.value;
        if (settings.iconEmoji && settings.iconUrl) {

            this.notificationService.error("Validation", "You cannot have a Emoji icon and a URL icon");
            return;
        }
        settings.notificationTemplates = this.templates;

        this.settingsService.saveSlackNotificationSettings(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Settings Saved", "Successfully saved the Slack settings");
            } else {
                this.notificationService.success("Settings Saved", "There was an error when saving the Slack settings");
            }
        });

    }

    test(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Validation", "Please check your entered values");
            return
        }

        var settings = <ISlackNotificationSettings>form.value;
        if (settings.iconEmoji && settings.iconUrl) {

            this.notificationService.error("Validation", "You cannot have a Emoji icon and a URL icon");
            return;
        }
        this.testerService.slackTest(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successful", "Successfully sent a Slack message, please check the discord channel");
            } else {
                this.notificationService.success("Error", "There was an error when sending the Slack message. Please check your settings");
            }
        })

    }
}