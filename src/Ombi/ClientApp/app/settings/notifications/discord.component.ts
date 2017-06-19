import { Component, OnInit } from '@angular/core';
import { FormGroup, Validators, FormBuilder } from '@angular/forms';

import { INotificationTemplates, IDiscordNotifcationSettings, NotificationType } from '../../interfaces/INotifcationSettings';
import { SettingsService } from '../../services/settings.service';
import { NotificationService } from "../../services/notification.service";
import { TesterService } from "../../services/applications/tester.service";

@Component({
    templateUrl: './discord.component.html',
})
export class DiscordComponent implements OnInit {
    constructor(private settingsService: SettingsService,
        private notificationService: NotificationService,
        private fb: FormBuilder,
        private testerService : TesterService) { }

    NotificationType = NotificationType;
    templates: INotificationTemplates[];

    form: FormGroup;

    ngOnInit(): void {
        this.settingsService.getDiscordNotificationSettings().subscribe(x => {
            this.templates = x.notificationTemplates;

            this.form = this.fb.group({
                enabled: [x.enabled],
                username: [x.username],
                webhookUrl: [x.webhookUrl, [Validators.required]],

            });
        });
    }

    onSubmit(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Validation", "Please check your entered values");
            return
        }

        var settings = <IDiscordNotifcationSettings>form.value;
        settings.notificationTemplates = this.templates;

        this.settingsService.saveDiscordNotificationSettings(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Settings Saved", "Successfully saved the Discord settings");
            } else {
                this.notificationService.success("Settings Saved", "There was an error when saving the Discord settings");
            }
        });

    }

    test(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Validation", "Please check your entered values");
            return
        }

        this.testerService.discordTest(form.value).subscribe(x => {
            if (x) {
                this.notificationService.success("Successful", "Successfully sent a Discord message, please check the discord channel");
            } else {
                this.notificationService.success("Error", "There was an error when sending the Discord message. Please check your settings");
            }
        })

    }
}