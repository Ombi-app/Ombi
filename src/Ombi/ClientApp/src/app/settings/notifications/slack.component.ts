import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";

import { INotificationTemplates, ISlackNotificationSettings, NotificationType } from "../../interfaces";
import { TesterService } from "../../services";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";

@Component({
    templateUrl: "./slack.component.html",
    styleUrls: ["./notificationtemplate.component.scss"]
})
export class SlackComponent implements OnInit {
    public NotificationType = NotificationType;
    public templates: INotificationTemplates[];
    public form: FormGroup;

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private fb: FormBuilder,
                private testerService: TesterService) { }

    public ngOnInit() {
        this.settingsService.getSlackNotificationSettings().subscribe(x => {
            this.templates = x.notificationTemplates;

            this.form = this.fb.group({
                enabled: [x.enabled],
                username: [x.username],
                webhookUrl: [x.webhookUrl, [Validators.required]],
                iconEmoji: [x.iconEmoji],
                iconUrl: [x.iconUrl],
                channel: [x.channel],

            });
        });
    }

    public onSubmit(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        const settings = <ISlackNotificationSettings> form.value;
        if (settings.iconEmoji && settings.iconUrl) {

            this.notificationService.error("You cannot have a Emoji icon and a URL icon");
            return;
        }
        settings.notificationTemplates = this.templates;

        this.settingsService.saveSlackNotificationSettings(settings).subscribe(x => {
            if (x) {
                this.notificationService.success( "Successfully saved the Slack settings");
            } else {
                this.notificationService.success( "There was an error when saving the Slack settings");
            }
        });

    }

    public test(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        const settings = <ISlackNotificationSettings> form.value;
        if (settings.iconEmoji && settings.iconUrl) {

            this.notificationService.error("You cannot have a Emoji icon and a URL icon");
            return;
        }
        this.testerService.slackTest(settings).subscribe(x => {
            if (x) {
                this.notificationService.success( "Successfully sent a Slack message, please check the slack channel");
            } else {
                this.notificationService.error("There was an error when sending the Slack message. Please check your settings");
            }
        });

    }
}
