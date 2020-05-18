import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";

import { INotificationTemplates, IWebhookNotificationSettings, NotificationType } from "../../interfaces";
import { TesterService } from "../../services";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";

@Component({
    templateUrl: "./webhook.component.html",
    styleUrls: ["./notificationtemplate.component.scss"]
})
export class WebhookComponent implements OnInit {
    public NotificationType = NotificationType;
    public templates: INotificationTemplates[];
    public form: FormGroup;

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private fb: FormBuilder,
                private testerService: TesterService) { }

    public ngOnInit() {
        this.settingsService.getWebhookNotificationSettings().subscribe(x => {
            this.form = this.fb.group({
                enabled: [x.enabled],
                webhookUrl: [x.webhookUrl, [Validators.required]],
                applicationToken: [x.applicationToken],
            });
        });
    }

    public onSubmit(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        const settings = <IWebhookNotificationSettings> form.value;

        this.settingsService.saveWebhookNotificationSettings(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved the Webhook settings");
            } else {
                this.notificationService.success("There was an error when saving the Webhook settings");
            }
        });

    }

    public test(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        this.testerService.webhookTest(form.value).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully sent a Webhook message");
            } else {
                this.notificationService.error("There was an error when sending the Webhook message. Please check your settings");
            }
        });
    }
}
